using TransactionService.Domain.Entities;

namespace TransactionService.Infrastructure.Persistence.Repositories.Interfaces;

public interface ITransactionRepo
{
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Transaction> Items, int TotalCount)> GetMyTransactionsAsync(
        Guid tenantPublicId,
        Guid userPublicId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    
    Task<Transaction?> GetByPublicIdAsync(
        Guid transactionPublicId,
        CancellationToken cancellationToken = default);
}