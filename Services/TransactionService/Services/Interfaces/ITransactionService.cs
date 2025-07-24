using SharedKernel.Common.Results;
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
}