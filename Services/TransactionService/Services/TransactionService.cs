using SharedKernel.Common.Results;
using TransactionService.Common.DTOs;
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
            TransStatus = riskResult == RiskStatus.Pass ? TransStatus.Success : TransStatus.Failed,
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
            ? ServiceResult<CreateTransactionResponse>.Ok(response, ResultCodes.Transaction.TransactionCreateSuccess, "Transaction Created")
            : ServiceResult<CreateTransactionResponse>.Fail(ResultCodes.Transaction.TransactionCreateFailed, "Transaction Failed");
    }

    public async Task<ServiceResult<TransactionDto>> QueryUserOwnTransactionByPublicIdAsync(string tenantPublicId, string userPublicId, string transactionPublicId)
    {
        var transaction = await transactionRepo.GetTransactionByPublicIdAsync(transactionPublicId);
        if (transaction == null)
        {
            return ServiceResult<TransactionDto>.Fail(ResultCodes.Transaction.TransactionNotFound, "Transaction Not Found");
        }

        if (transaction.UserPublicId != userPublicId 
            || transaction.TenantPublicId != tenantPublicId)
        {
            return ServiceResult<TransactionDto>.Fail(ResultCodes.Transaction.TransactionNotBelongToCurrentUser, "Transaction Not belong to user");
        }

        var dto = new TransactionDto(transaction.TransactionPublicId.ToString(),
            transaction.TenantPublicId, 
            transaction.UserPublicId,
            transaction.Amount,
            transaction.Currency,
            transaction.TransStatus,
            transaction.RiskStatus,
            transaction.Description,
            transaction.CreatedAt);
        return ServiceResult<TransactionDto>.Ok(dto, ResultCodes.Transaction.TransactionQuerySuccess, "Transaction Query Success");
    }
}