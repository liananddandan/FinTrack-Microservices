using SharedKernel.Common.Results;
using TransactionService.Api.Contracts;
using TransactionService.Application.Common.DTOs;

namespace TransactionService.Application.Abstractions;

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

    Task<ServiceResult<CreateTransactionResult>> CreateProcurementAsync(
        string tenantPublicId,
        string createdByUserPublicId,
        CreateProcurementRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> UpdateProcurementAsync(
        string tenantPublicId,
        string userPublicId,
        string transactionPublicId,
        UpdateProcurementRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> SubmitProcurementAsync(
        string tenantPublicId,
        string userPublicId,
        string transactionPublicId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> ApproveProcurementAsync(
        string tenantPublicId,
        string role,
        string transactionPublicId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> RejectProcurementAsync(
        string tenantPublicId,
        string role,
        string transactionPublicId,
        RejectProcurementRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<PagedResult<TransactionListItemDto>>> GetMyTransactionsAsync(
        string tenantPublicId,
        string userPublicId,
        int pageNumber,
        int pageSize,
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

    Task<ServiceResult<TransactionDetailDto>> GetTransactionDetailAsync(
        string tenantPublicId,
        string userPublicId,
        string role,
        string transactionPublicId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<TenantTransactionSummaryDto>> GetTransactionSummaryAsync(
        string tenantPublicId,
        string role,
        CancellationToken cancellationToken = default);
}