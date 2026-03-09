using Microsoft.EntityFrameworkCore;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;
using TransactionService.Infrastructure.Persistence.Repositories.Interfaces;
using TransactionService.Infrastructure.Persistence.Repositories.Models;

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
    
    public async Task<TenantTransactionSummaryModel> GetTransactionSummaryAsync(
        Guid tenantPublicId,
        CancellationToken cancellationToken = default)
    {
        var transactions = dbContext.Transactions
            .AsNoTracking()
            .Where(x => x.TenantPublicId == tenantPublicId);

        var tenantName = await transactions
            .Select(x => x.TenantNameSnapshot)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        var totalDonationAmount = await transactions
            .Where(x => x.Type == TransactionType.Donation &&
                        x.PaymentStatus == PaymentStatus.Succeeded)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var totalProcurementAmount = await transactions
            .Where(x => x.Type == TransactionType.Procurement &&
                        x.PaymentStatus == PaymentStatus.Succeeded)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var totalTransactionCount = await transactions.CountAsync(cancellationToken);

        return new TenantTransactionSummaryModel
        {
            TenantName = tenantName,
            CurrentBalance = totalDonationAmount - totalProcurementAmount,
            TotalDonationAmount = totalDonationAmount,
            TotalProcurementAmount = totalProcurementAmount,
            TotalTransactionCount = totalTransactionCount
        };
    }
    
    public async Task<Transaction?> GetProcurementForOwnerAsync(
        Guid tenantPublicId,
        Guid userPublicId,
        Guid transactionPublicId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Transactions
            .FirstOrDefaultAsync(x =>
                    x.PublicId == transactionPublicId &&
                    x.TenantPublicId == tenantPublicId &&
                    x.CreatedByUserPublicId == userPublicId &&
                    x.Type == TransactionType.Procurement,
                cancellationToken);
    }

    public async Task<Transaction?> GetProcurementForTenantAsync(
        Guid tenantPublicId,
        Guid transactionPublicId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Transactions
            .FirstOrDefaultAsync(x =>
                    x.PublicId == transactionPublicId &&
                    x.TenantPublicId == tenantPublicId &&
                    x.Type == TransactionType.Procurement,
                cancellationToken);
    }

    public async Task<Transaction?> GetByTenantAndTitleAsync(
        Guid tenantPublicId,
        string title,
        CancellationToken cancellationToken = default)
    {
        title = title.Trim();

        return await dbContext.Transactions
            .FirstOrDefaultAsync(
                x => x.TenantPublicId == tenantPublicId && x.Title == title,
                cancellationToken);
    }
}
