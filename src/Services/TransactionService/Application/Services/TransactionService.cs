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

    public async Task<ServiceResult<PagedResult<TransactionListItemDto>>> GetTransactionsAsync(
        string tenantPublicId,
        string role,
        string? type,
        string? status,
        string? paymentStatus,
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

        if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult<PagedResult<TransactionListItemDto>>.Fail(
                ResultCodes.Transaction.TransactionNotBelongToCurrentUser,
                "Only admin can query tenant transactions.");
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
            var (items, totalCount) = await transactionRepo.GetTransactionsAsync(
                parsedTenantPublicId,
                type,
                status,
                paymentStatus,
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
                "Tenant transactions retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to query tenant transactions. Tenant={TenantPublicId}",
                tenantPublicId);

            return ServiceResult<PagedResult<TransactionListItemDto>>.Fail(
                ResultCodes.Transaction.TransactionQueryFailed,
                "Failed to query tenant transactions.");
        }
    }

    public async Task<ServiceResult<TenantTransactionSummaryDto>> GetTransactionSummaryAsync(
        string tenantPublicId,
        string role,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantPublicId) ||
            !Guid.TryParse(tenantPublicId, out var parsedTenantPublicId))
        {
            return ServiceResult<TenantTransactionSummaryDto>.Fail(
                ResultCodes.Transaction.TransactionQueryFailed,
                "Tenant is invalid.");
        }

        if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult<TenantTransactionSummaryDto>.Fail(
                ResultCodes.Transaction.TransactionNotBelongToCurrentUser,
                "Only admin can query tenant summary.");
        }

        try
        {
            var summary = await transactionRepo.GetTransactionSummaryAsync(
                parsedTenantPublicId,
                cancellationToken);

            var result = new TenantTransactionSummaryDto
            {
                TenantPublicId = parsedTenantPublicId.ToString(),
                TenantName = summary.TenantName,
                CurrentBalance = summary.CurrentBalance,
                TotalDonationAmount = summary.TotalDonationAmount,
                TotalProcurementAmount = summary.TotalProcurementAmount,
                TotalTransactionCount = summary.TotalTransactionCount
            };

            return ServiceResult<TenantTransactionSummaryDto>.Ok(
                result,
                ResultCodes.Transaction.TransactionQuerySuccess,
                "Tenant transaction summary retrieved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to query tenant transaction summary. Tenant={TenantPublicId}",
                tenantPublicId);

            return ServiceResult<TenantTransactionSummaryDto>.Fail(
                ResultCodes.Transaction.TransactionQueryFailed,
                "Failed to query tenant transaction summary.");
        }
    }

    public async Task<ServiceResult<CreateTransactionResult>> CreateProcurementAsync(
        string tenantPublicId,
        string createdByUserPublicId,
        CreateProcurementRequest request,
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
            !Guid.TryParse(createdByUserPublicId, out var parsedUserPublicId))
        {
            return ServiceResult<CreateTransactionResult>.Fail(
                ResultCodes.Transaction.TransactionCreateFailed,
                "User is invalid.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return ServiceResult<CreateTransactionResult>.Fail(
                ResultCodes.Transaction.TransactionCreateFailed,
                "Title is required.");
        }

        if (request.Amount <= 0)
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

            var transaction = new Transaction
            {
                TenantPublicId = parsedTenantPublicId,
                TenantNameSnapshot = tenantResult.Data.TenantName,
                CreatedByUserPublicId = parsedUserPublicId,
                Type = TransactionType.Procurement,
                Title = request.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description)
                    ? null
                    : request.Description.Trim(),
                Amount = request.Amount,
                Currency = string.IsNullOrWhiteSpace(request.Currency)
                    ? "NZD"
                    : request.Currency.Trim().ToUpperInvariant(),
                Status = TransactionStatus.Draft,
                PaymentStatus = PaymentStatus.NotStarted,
                RiskStatus = RiskStatus.NotChecked,
                CreatedAtUtc = DateTime.UtcNow
            };

            await transactionRepo.AddAsync(transaction, cancellationToken);
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
                    PaymentStatus = transaction.PaymentStatus.ToString()
                },
                ResultCodes.Transaction.ProcurementCreateSuccess,
                "Procurement draft created successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to create procurement transaction for tenant {TenantPublicId}",
                tenantPublicId);

            return ServiceResult<CreateTransactionResult>.Fail(
                ResultCodes.Transaction.TransactionCreateFailed,
                "Failed to create procurement transaction.");
        }
    }

    public async Task<ServiceResult<bool>> UpdateProcurementAsync(
        string tenantPublicId,
        string userPublicId,
        string transactionPublicId,
        UpdateProcurementRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantPublicId) ||
            !Guid.TryParse(tenantPublicId, out var parsedTenantPublicId))
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Transaction.TransactionQueryFailed,
                "Tenant is invalid.");
        }

        if (string.IsNullOrWhiteSpace(userPublicId) ||
            !Guid.TryParse(userPublicId, out var parsedUserPublicId))
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Transaction.TransactionQueryFailed,
                "User is invalid.");
        }

        if (string.IsNullOrWhiteSpace(transactionPublicId) ||
            !Guid.TryParse(transactionPublicId, out var parsedTransactionPublicId))
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Transaction.TransactionQueryFailed,
                "Transaction public id is invalid.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Transaction.TransactionCreateFailed,
                "Title is required.");
        }

        if (request.Amount <= 0)
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Transaction.TransactionCreateFailed,
                "Amount must be greater than zero.");
        }

        try
        {
            var transaction = await transactionRepo.GetProcurementForOwnerAsync(
                parsedTenantPublicId,
                parsedUserPublicId,
                parsedTransactionPublicId,
                cancellationToken);

            if (transaction == null)
            {
                return ServiceResult<bool>.Fail(
                    ResultCodes.Transaction.TransactionNotFound,
                    "Procurement transaction not found.");
            }

            if (transaction.Status != TransactionStatus.Draft)
            {
                return ServiceResult<bool>.Fail(
                    ResultCodes.Transaction.TransactionCreateFailed,
                    "Only draft procurement can be updated.");
            }

            transaction.Title = request.Title.Trim();
            transaction.Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim();
            transaction.Amount = request.Amount;
            transaction.Currency = string.IsNullOrWhiteSpace(request.Currency)
                ? "NZD"
                : request.Currency.Trim().ToUpperInvariant();

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult<bool>.Ok(
                true,
                ResultCodes.Transaction.ProcurementUpdateSuccess,
                "Procurement draft updated successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to update procurement transaction {TransactionPublicId}",
                transactionPublicId);

            return ServiceResult<bool>.Fail(
                ResultCodes.Transaction.TransactionQueryFailed,
                "Failed to update procurement transaction.");
        }
    }

    public async Task<ServiceResult<bool>> SubmitProcurementAsync(
        string tenantPublicId,
        string userPublicId,
        string transactionPublicId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantPublicId) ||
            !Guid.TryParse(tenantPublicId, out var parsedTenantPublicId))
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Transaction.TransactionQueryFailed,
                "Tenant is invalid.");
        }

        if (string.IsNullOrWhiteSpace(userPublicId) ||
            !Guid.TryParse(userPublicId, out var parsedUserPublicId))
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Transaction.TransactionQueryFailed,
                "User is invalid.");
        }

        if (string.IsNullOrWhiteSpace(transactionPublicId) ||
            !Guid.TryParse(transactionPublicId, out var parsedTransactionPublicId))
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Transaction.TransactionQueryFailed,
                "Transaction public id is invalid.");
        }

        try
        {
            var transaction = await transactionRepo.GetProcurementForOwnerAsync(
                parsedTenantPublicId,
                parsedUserPublicId,
                parsedTransactionPublicId,
                cancellationToken);

            if (transaction == null)
            {
                return ServiceResult<bool>.Fail(
                    ResultCodes.Transaction.TransactionNotFound,
                    "Procurement transaction not found.");
            }

            if (transaction.Status != TransactionStatus.Draft)
            {
                return ServiceResult<bool>.Fail(
                    ResultCodes.Transaction.TransactionCreateFailed,
                    "Only draft procurement can be submitted.");
            }

            transaction.Status = TransactionStatus.Submitted;

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult<bool>.Ok(
                true,
                ResultCodes.Transaction.ProcurementSubmitSuccess,
                "Procurement submitted successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to submit procurement transaction {TransactionPublicId}",
                transactionPublicId);

            return ServiceResult<bool>.Fail(
                ResultCodes.Transaction.TransactionQueryFailed,
                "Failed to submit procurement transaction.");
        }
    }

    public async Task<ServiceResult<bool>> ApproveProcurementAsync(
        string tenantPublicId,
        string role,
        string transactionPublicId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantPublicId) ||
            !Guid.TryParse(tenantPublicId, out var parsedTenantPublicId))
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Transaction.TransactionQueryFailed,
                "Tenant is invalid.");
        }

        if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Transaction.TransactionNotBelongToCurrentUser,
                "Only admin can approve procurement.");
        }

        if (string.IsNullOrWhiteSpace(transactionPublicId) ||
            !Guid.TryParse(transactionPublicId, out var parsedTransactionPublicId))
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Transaction.TransactionQueryFailed,
                "Transaction public id is invalid.");
        }

        try
        {
            var transaction = await transactionRepo.GetProcurementForTenantAsync(
                parsedTenantPublicId,
                parsedTransactionPublicId,
                cancellationToken);

            if (transaction == null)
            {
                return ServiceResult<bool>.Fail(
                    ResultCodes.Transaction.TransactionNotFound,
                    "Procurement transaction not found.");
            }

            if (transaction.Status != TransactionStatus.Submitted)
            {
                return ServiceResult<bool>.Fail(
                    ResultCodes.Transaction.TransactionCreateFailed,
                    "Only submitted procurement can be approved.");
            }

            transaction.Status = TransactionStatus.Approved;

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult<bool>.Ok(
                true,
                ResultCodes.Transaction.ProcurementApproveSuccess,
                "Procurement approved successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to approve procurement transaction {TransactionPublicId}",
                transactionPublicId);

            return ServiceResult<bool>.Fail(
                ResultCodes.Transaction.TransactionQueryFailed,
                "Failed to approve procurement transaction.");
        }
    }

    public async Task<ServiceResult<bool>> RejectProcurementAsync(
        string tenantPublicId,
        string role,
        string transactionPublicId,
        RejectProcurementRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantPublicId) ||
            !Guid.TryParse(tenantPublicId, out var parsedTenantPublicId))
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Transaction.TransactionQueryFailed,
                "Tenant is invalid.");
        }

        if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Transaction.TransactionNotBelongToCurrentUser,
                "Only admin can reject procurement.");
        }

        if (string.IsNullOrWhiteSpace(transactionPublicId) ||
            !Guid.TryParse(transactionPublicId, out var parsedTransactionPublicId))
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Transaction.TransactionQueryFailed,
                "Transaction public id is invalid.");
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Transaction.TransactionCreateFailed,
                "Reject reason is required.");
        }

        try
        {
            var transaction = await transactionRepo.GetProcurementForTenantAsync(
                parsedTenantPublicId,
                parsedTransactionPublicId,
                cancellationToken);

            if (transaction == null)
            {
                return ServiceResult<bool>.Fail(
                    ResultCodes.Transaction.TransactionNotFound,
                    "Procurement transaction not found.");
            }

            if (transaction.Status != TransactionStatus.Submitted)
            {
                return ServiceResult<bool>.Fail(
                    ResultCodes.Transaction.TransactionCreateFailed,
                    "Only submitted procurement can be rejected.");
            }

            transaction.Status = TransactionStatus.Rejected;
            transaction.FailureReason = request.Reason.Trim();

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult<bool>.Ok(
                true,
                ResultCodes.Transaction.ProcurementRejectSuccess,
                "Procurement rejected successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to reject procurement transaction {TransactionPublicId}",
                transactionPublicId);

            return ServiceResult<bool>.Fail(
                ResultCodes.Transaction.TransactionQueryFailed,
                "Failed to reject procurement transaction.");
        }
    }
}