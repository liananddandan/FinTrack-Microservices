using Microsoft.EntityFrameworkCore;
using TransactionService.Domain.Entities;
using TransactionService.Infrastructure;
using TransactionService.Repositories.Interfaces;

namespace TransactionService.Repositories;

public class TransactionRepo(TransactionDbContext dbContext) : ITransactionRepo
{
    public async Task AddTransactionAsync(Transaction transaction)
    {
        await dbContext.Transactions.AddAsync(transaction);
    }

    public async Task<Transaction?> GetTransactionByPublicIdAsync(string transactionPublicId)
    {
        return await dbContext.Transactions
            .Where(t => t.TransactionPublicId.ToString().Equals(transactionPublicId))
            .FirstOrDefaultAsync();
    }

    public async Task<(List<Transaction>, int)> GetTransactionsByPageAsync(
        string tenantPublicId, string userPublicId, DateTime? startDate, DateTime? endDate,
        int page, int pageSize, string sortBy)
    {
        var query = dbContext.Transactions
            .Where(t => t.TenantPublicId.ToString().Equals(tenantPublicId))
            .Where(t => t.UserPublicId.ToString().Equals(userPublicId));
        if (startDate is not null)
        {
            query.Where(t => t.CreatedAt >= startDate);
        }

        if (endDate is not null)
        {
            query.Where(t => t.CreatedAt <= endDate);
        }

        if (sortBy.Equals("desc"))
        {
            query.OrderByDescending(t => t.CreatedAt);
        }
        else
        {
            query.OrderBy(t => t.CreatedAt);
        }

        page = page > 0 ? page : 1;
        pageSize = pageSize > 0 ? pageSize : 10;
        var totalCount = await query.CountAsync();
        var data = await query.Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (data, totalCount);
    }
}