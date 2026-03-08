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
}