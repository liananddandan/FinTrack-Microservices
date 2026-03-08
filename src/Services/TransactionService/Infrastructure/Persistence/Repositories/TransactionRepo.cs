using TransactionService.Domain.Entities;
using TransactionService.Infrastructure.Persistence.Repositories.Interfaces;

namespace TransactionService.Infrastructure.Persistence.Repositories;

public class TransactionRepo(TransactionDbContext dbContext) : ITransactionRepo
{
    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await dbContext.Transactions.AddAsync(transaction, cancellationToken);
    }
}