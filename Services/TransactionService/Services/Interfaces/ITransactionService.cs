using SharedKernel.Common.Results;
using TransactionService.Common.DTOs;
using TransactionService.Common.Requests;
using TransactionService.Common.Responses;

namespace TransactionService.Services.Interfaces;

public interface ITransactionService
{
    public Task<ServiceResult<CreateTransactionResponse>> CreateTransactionAsync(string tenantPublicId,
        string userPublicId,
        decimal amount,
        string currency,
        string? description);

    public Task<ServiceResult<TransactionDto>> QueryUserOwnTransactionByPublicIdAsync(string tenantPublicId, 
        string userPublicId,
        string transactionPublicId);
    
    public Task<ServiceResult<QueryByPageDto>> QueryTransactionByPageAsync(
        string tenantPublicId,
        string userPublicId,
        DateTime? startDate,
        DateTime? endDate,
        int page,
        int pageSize,
        string sortBy);
}