using Microsoft.EntityFrameworkCore;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;
using TransactionService.Infrastructure.Persistence.Repositories.Interfaces;

namespace TransactionService.Infrastructure.Persistence.Repositories;

public class TransactionRepo(TransactionDbContext dbContext) : ITransactionRepo
{
    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await dbContext.Transactions.AddAsync(transaction, cancellationToken);
    }
    
    public async Task<(IReadOnlyList<Transaction> Items, int TotalCount)> GetMyTransactionsAsync(
        Guid tenantPublicId,
        Guid userPublicId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Transactions
            .AsNoTracking()
            .Where(x => x.TenantPublicId == tenantPublicId &&
                        x.CreatedByUserPublicId == userPublicId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
    
    public async Task<Transaction?> GetByPublicIdAsync(
        Guid transactionPublicId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.PublicId == transactionPublicId, cancellationToken);
    }
    
    public async Task<(IReadOnlyList<Transaction> Items, int TotalCount)> GetTransactionsAsync(
        Guid tenantPublicId,
        string? type,
        string? status,
        string? paymentStatus,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Transactions
            .AsNoTracking()
            .Where(x => x.TenantPublicId == tenantPublicId);

        if (!string.IsNullOrWhiteSpace(type) &&
            Enum.TryParse<TransactionType>(type, true, out var parsedType))
        {
            query = query.Where(x => x.Type == parsedType);
        }

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<TransactionStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(x => x.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(paymentStatus) &&
            Enum.TryParse<PaymentStatus>(paymentStatus, true, out var parsedPaymentStatus))
        {
            query = query.Where(x => x.PaymentStatus == parsedPaymentStatus);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}