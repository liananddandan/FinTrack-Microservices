using SharedKernel.Common.Results;
using TransactionService.Common.Responses;
using TransactionService.Common.Status;
using TransactionService.Domain.Entities;
using TransactionService.Repositories.Interfaces;
using TransactionService.Services.Interfaces;

namespace TransactionService.Services;

public class TransactionService(
    ITransactionRepo transactionRepo,
    IUnitOfWork unitOfWork,
    IRiskService riskService) : ITransactionService
{
    public async Task<ServiceResult<CreateTransactionResponse>> CreateTransactionAsync(string tenantPublicId, string userPublicId, decimal amount, string currency,
        string? description)
    {
        var riskResult = await riskService.CheckRiskAsync(tenantPublicId, userPublicId, amount, currency);

        var transaction = new Transaction()
        {
            TenantPublicId = tenantPublicId,
            UserPublicId = userPublicId,
            Amount = amount,
            Currency = currency,
            Description = description,
            TransStatus = riskResult == RiskStatus.Pass ? TransactionStatus.Success : TransactionStatus.Failed,
            RiskStatus = riskResult
        };
        
        await transactionRepo.AddTransactionAsync(transaction);
        await unitOfWork.SaveChangesAsync();
        var response = new CreateTransactionResponse(
            transaction.TransactionPublicId.ToString(),
            transaction.Amount,
            transaction.Currency,
            transaction.TransStatus,
            transaction.RiskStatus,
            transaction.CreatedAt);
        return riskResult == RiskStatus.Pass
            ? ServiceResult<CreateTransactionResponse>.Ok(response, ResultCodes.Transaction.TransactionSuccess, "Transaction Created")
            : ServiceResult<CreateTransactionResponse>.Fail(ResultCodes.Transaction.TransactionFailed, "Transaction Failed");
    }
}