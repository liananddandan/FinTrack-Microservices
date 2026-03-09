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
    
    Task<ServiceResult<TransactionDetailDto>> GetTransactionDetailAsync(
        string tenantPublicId,
        string userPublicId,
        string role,
        string transactionPublicId,
        CancellationToken cancellationToken = default);
    
    Task<ServiceResult<PagedResult<TransactionListItemDto>>> GetTransactionsAsync(
        string tenantPublicId,
        string role,
        string? type,
        string? status,
        string? paymentStatus,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    
    Task<ServiceResult<TenantTransactionSummaryDto>> GetTransactionSummaryAsync(
        string tenantPublicId,
        string role,
        CancellationToken cancellationToken = default);
}