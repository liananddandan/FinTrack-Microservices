using TransactionService.Domain.Entities;

namespace TransactionService.Repositories.Interfaces;

public interface ITransactionRepo
{
    public Task AddTransactionAsync(Transaction transaction);
}