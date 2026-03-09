using SharedKernel.Common.Results;
using TransactionService.Application.Common.DTOs;
using TransactionService.Application.Services.Interfaces;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;
using TransactionService.Infrastructure.Persistence.Repositories.Interfaces;

namespace TransactionService.Application.Services;

public class TransactionService(
    ITransactionRepo transactionRepo,
    ITenantAccountRepo tenantAccountRepo,
    ITenantInfoClient tenantInfoClient,
    IPaymentGateway paymentGateway,
    IUnitOfWork unitOfWork,
    ILogger<TransactionService> logger
) : ITransactionService
{
    public async Task<ServiceResult<CreateTransactionResult>> CreateDonationAsync(
        string tenantPublicId,
        string createdByUserPublicId,
        string title,
        string? description,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantPublicId) ||
            !Guid.TryParse(tenantPublicId, out var parsedTenantPublicId))
        {
            return ServiceResult<CreateTransactionResult>.Fail(
                ResultCodes.Transaction.TransactionCreateFailed,
                "Tenant is invalid.");
        }

        if (string.IsNullOrWhiteSpace(createdByUserPublicId) ||
            !Guid.TryParse(createdByUserPublicId, out var parsedCreatedByUserPublicId))
        {
            return ServiceResult<CreateTransactionResult>.Fail(
                ResultCodes.Transaction.TransactionCreateFailed,
                "User is invalid.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            return ServiceResult<CreateTransactionResult>.Fail(
                ResultCodes.Transaction.TransactionCreateFailed,
                "Title is required.");
        }

        if (amount <= 0)
        {
            return ServiceResult<CreateTransactionResult>.Fail(
                ResultCodes.Transaction.TransactionCreateFailed,
                "Amount must be greater than zero.");
        }

        try
        {
            var tenantResult = await tenantInfoClient.GetTenantSummaryAsync(
                tenantPublicId,
                cancellationToken);

            if (!tenantResult.Success || tenantResult.Data == null)
            {
                return ServiceResult<CreateTransactionResult>.Fail(
                    ResultCodes.Transaction.TransactionCreateFailed,
                    "Tenant information could not be resolved.");
            }

            var tenant = tenantResult.Data;

            var transaction = new Domain.Entities.Transaction
            {
                TenantPublicId = parsedTenantPublicId,
                TenantNameSnapshot = tenant.TenantName,
                CreatedByUserPublicId = parsedCreatedByUserPublicId,
                Type = TransactionType.Donation,
                Title = title.Trim(),
                Description = string.IsNullOrWhiteSpace(description)
                    ? null
                    : description.Trim(),
                Amount = amount,
                Currency = string.IsNullOrWhiteSpace(currency)
                    ? "NZD"
                    : currency.Trim().ToUpperInvariant(),
                Status = TransactionStatus.Draft,
                PaymentStatus = PaymentStatus.Processing,
                RiskStatus = RiskStatus.NotChecked,
                CreatedAtUtc = DateTime.UtcNow
            };

            await transactionRepo.AddAsync(transaction, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var paymentResult = await paymentGateway.PayAsync(
                new PaymentExecutionRequest
                {
                    TransactionPublicId = transaction.PublicId.ToString(),
                    TenantPublicId = tenantPublicId,
                    Amount = transaction.Amount,
                    Currency = transaction.Currency,
                    Type = transaction.Type.ToString()
                },
                cancellationToken);

            if (paymentResult.Success)
            {
                transaction.PaymentStatus = PaymentStatus.Succeeded;
                transaction.Status = TransactionStatus.Completed;
                transaction.PaidByUserPublicId = parsedCreatedByUserPublicId;
                transaction.PaidAtUtc = DateTime.UtcNow;
                transaction.PaymentReference = paymentResult.PaymentReference;
                transaction.FailureReason = null;

                var account = await tenantAccountRepo.GetByTenantPublicIdAsync(
                    parsedTenantPublicId,
                    cancellationToken);

                if (account == null)
                {
                    account = new TenantAccount
                    {
                        TenantPublicId = parsedTenantPublicId,
                        AvailableBalance = transaction.Amount,
                        UpdatedAtUtc = DateTime.UtcNow
                    };

                    await tenantAccountRepo.AddAsync(account, cancellationToken);
                }
                else
                {
                    account.AvailableBalance += transaction.Amount;
                    account.UpdatedAtUtc = DateTime.UtcNow;
                }
            }
            else
            {
                transaction.PaymentStatus = PaymentStatus.Failed;
                transaction.Status = TransactionStatus.Failed;
                transaction.FailureReason = paymentResult.FailureReason;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult<CreateTransactionResult>.Ok(
                new CreateTransactionResult
                {
                    TransactionPublicId = transaction.PublicId.ToString(),
                    TenantPublicId = transaction.TenantPublicId.ToString(),
                    TenantName = transaction.TenantNameSnapshot,
                    Type = transaction.Type.ToString(),
                    Amount = transaction.Amount,
                    Currency = transaction.Currency,
                    Status = transaction.Status.ToString(),
                    PaymentStatus = transaction.PaymentStatus.ToString(),
                    PaymentReference = transaction.PaymentReference,
                    FailureReason = transaction.FailureReason
                },
                ResultCodes.Transaction.TransactionCreateSuccess,
                "Donation transaction processed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to create donation transaction for tenant {TenantPublicId}",
                tenantPublicId);

            return ServiceResult<CreateTransactionResult>.Fail(
                ResultCodes.Transaction.TransactionCreateFailed,
                "Failed to create donation transaction.");
        }
    }
    
    public async Task<ServiceResult<PagedResult<TransactionListItemDto>>> GetMyTransactionsAsync(
        string tenantPublicId,
        string userPublicId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantPublicId) ||
            !Guid.TryParse(tenantPublicId, out var parsedTenantPublicId))
        {
            return ServiceResult<PagedResult<TransactionListItemDto>>.Fail(
                ResultCodes.Transaction.TransactionQueryFailed,
                "Tenant is invalid.");
        }

        if (string.IsNullOrWhiteSpace(userPublicId) ||
            !Guid.TryParse(userPublicId, out var parsedUserPublicId))
        {
            return ServiceResult<PagedResult<TransactionListItemDto>>.Fail(
                ResultCodes.Transaction.TransactionQueryFailed,
                "User is invalid.");
        }

        if (pageNumber <= 0)
        {
            pageNumber = 1;
        }

        if (pageSize <= 0)
        {
            pageSize = 10;
        }

        if (pageSize > 100)
        {
            pageSize = 100;
        }

        try
        {
            var (items, totalCount) = await transactionRepo.GetMyTransactionsAsync(
                parsedTenantPublicId,
                parsedUserPublicId,
                pageNumber,
                pageSize,
                cancellationToken);

            var result = new PagedResult<TransactionListItemDto>
            {
                Items = items.Select(x => new TransactionListItemDto
                {
                    TransactionPublicId = x.PublicId.ToString(),
                    TenantPublicId = x.TenantPublicId.ToString(),
                    TenantName = x.TenantNameSnapshot,
                    Type = x.Type.ToString(),
                    Title = x.Title,
                    Amount = x.Amount,
                    Currency = x.Currency,
                    Status = x.Status.ToString(),
                    PaymentStatus = x.PaymentStatus.ToString(),
                    RiskStatus = x.RiskStatus.ToString(),
                    CreatedAtUtc = x.CreatedAtUtc
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<TransactionListItemDto>>.Ok(
                result,
                ResultCodes.Transaction.TransactionQueryByPageSuccess,
                "Transactions retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to query transactions for tenant {TenantPublicId}, user {UserPublicId}",
                tenantPublicId,
                userPublicId);

            return ServiceResult<PagedResult<TransactionListItemDto>>.Fail(
                ResultCodes.Transaction.TransactionQueryFailed,
                "Failed to query transactions.");
        }
    }
    
    public async Task<ServiceResult<TransactionDetailDto>> GetTransactionDetailAsync(
        string tenantPublicId,
        string userPublicId,
        string role,
        string transactionPublicId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantPublicId) ||
            !Guid.TryParse(tenantPublicId, out var parsedTenantPublicId))
        {
            return ServiceResult<TransactionDetailDto>.Fail(
                ResultCodes.Transaction.TransactionQueryFailed,
                "Tenant is invalid.");
        }

        if (string.IsNullOrWhiteSpace(userPublicId) ||
            !Guid.TryParse(userPublicId, out var parsedUserPublicId))
        {
            return ServiceResult<TransactionDetailDto>.Fail(
                ResultCodes.Transaction.TransactionQueryFailed,
                "User is invalid.");
        }

        if (string.IsNullOrWhiteSpace(transactionPublicId) ||
            !Guid.TryParse(transactionPublicId, out var parsedTransactionPublicId))
        {
            return ServiceResult<TransactionDetailDto>.Fail(
                ResultCodes.Transaction.TransactionQueryFailed,
                "Transaction public id is invalid.");
        }

        try
        {
            var transaction = await transactionRepo.GetByPublicIdAsync(
                parsedTransactionPublicId,
                cancellationToken);

            if (transaction == null)
            {
                return ServiceResult<TransactionDetailDto>.Fail(
                    ResultCodes.Transaction.TransactionNotFound,
                    "Transaction not found.");
            }

            if (transaction.TenantPublicId != parsedTenantPublicId)
            {
                return ServiceResult<TransactionDetailDto>.Fail(
                    ResultCodes.Transaction.TransactionNotFound,
                    "Transaction not found.");
            }

            var isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);

            if (!isAdmin && transaction.CreatedByUserPublicId != parsedUserPublicId)
            {
                return ServiceResult<TransactionDetailDto>.Fail(
                    ResultCodes.Transaction.TransactionNotBelongToCurrentUser,
                    "Transaction does not belong to current user.");
            }

            var result = new TransactionDetailDto
            {
                TransactionPublicId = transaction.PublicId.ToString(),
                TenantPublicId = transaction.TenantPublicId.ToString(),
                TenantName = transaction.TenantNameSnapshot,
                Type = transaction.Type.ToString(),
                Title = transaction.Title,
                Description = transaction.Description,
                Amount = transaction.Amount,
                Currency = transaction.Currency,
                Status = transaction.Status.ToString(),
                PaymentStatus = transaction.PaymentStatus.ToString(),
                RiskStatus = transaction.RiskStatus.ToString(),
                CreatedByUserPublicId = transaction.CreatedByUserPublicId.ToString(),
                CreatedAtUtc = transaction.CreatedAtUtc,
                ApprovedByUserPublicId = transaction.ApprovedByUserPublicId?.ToString(),
                ApprovedAtUtc = transaction.ApprovedAtUtc,
                PaidByUserPublicId = transaction.PaidByUserPublicId?.ToString(),
                PaidAtUtc = transaction.PaidAtUtc,
                PaymentReference = transaction.PaymentReference,
                FailureReason = transaction.FailureReason,
                RefundedByUserPublicId = transaction.RefundedByUserPublicId?.ToString(),
                RefundedAtUtc = transaction.RefundedAtUtc
            };

            return ServiceResult<TransactionDetailDto>.Ok(
                result,
                ResultCodes.Transaction.TransactionQuerySuccess,
                "Transaction retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to query transaction detail. Tenant={TenantPublicId}, User={UserPublicId}, Transaction={TransactionPublicId}",
                tenantPublicId,
                userPublicId,
                transactionPublicId);

            return ServiceResult<TransactionDetailDto>.Fail(
                ResultCodes.Transaction.TransactionQueryFailed,
                "Failed to query transaction.");
        }
    }
}