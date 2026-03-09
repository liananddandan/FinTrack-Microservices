using SharedKernel.Common.Results;
using TransactionService.Application.Common.DTOs;

namespace TransactionService.Application.Services.Interfaces;

public interface ITransactionService
{
    Task<ServiceResult<CreateTransactionResult>> CreateDonationAsync(
        string tenantPublicId,
        string createdByUserPublicId,
        string title,
        string? description,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default);
    
    Task<ServiceResult<PagedResult<TransactionListItemDto>>> GetMyTransactionsAsync(
        string tenantPublicId,
        string userPublicId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}