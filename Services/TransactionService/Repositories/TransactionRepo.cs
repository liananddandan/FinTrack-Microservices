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
}