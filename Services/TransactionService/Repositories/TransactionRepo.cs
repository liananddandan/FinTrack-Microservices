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
}