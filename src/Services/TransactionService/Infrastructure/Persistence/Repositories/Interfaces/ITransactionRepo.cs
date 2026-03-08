using TransactionService.Domain.Entities;

namespace TransactionService.Infrastructure.Persistence.Repositories.Interfaces;

public interface ITransactionRepo
{
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);

}