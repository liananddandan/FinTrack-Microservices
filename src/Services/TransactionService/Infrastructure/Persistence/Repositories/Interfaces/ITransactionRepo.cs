using TransactionService.Domain.Entities;
using TransactionService.Infrastructure.Persistence.Repositories.Models;

namespace TransactionService.Infrastructure.Persistence.Repositories.Interfaces;

public interface ITransactionRepo
{
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);

    Task<Transaction?> GetByPublicIdAsync(
        Guid transactionPublicId,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Transaction> Items, int TotalCount)> GetMyTransactionsAsync(
        Guid tenantPublicId,
        Guid userPublicId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Transaction> Items, int TotalCount)> GetTransactionsAsync(
        Guid tenantPublicId,
        string? type,
        string? status,
        string? paymentStatus,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<TenantTransactionSummaryModel> GetTransactionSummaryAsync(
        Guid tenantPublicId,
        CancellationToken cancellationToken = default);

    Task<Transaction?> GetProcurementForOwnerAsync(
        Guid tenantPublicId,
        Guid userPublicId,
        Guid transactionPublicId,
        CancellationToken cancellationToken = default);

    Task<Transaction?> GetProcurementForTenantAsync(
        Guid tenantPublicId,
        Guid transactionPublicId,
        CancellationToken cancellationToken = default);
}