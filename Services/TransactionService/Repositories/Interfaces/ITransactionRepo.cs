using TransactionService.Domain.Entities;

namespace TransactionService.Repositories.Interfaces;

public interface ITransactionRepo
{
    public Task AddTransactionAsync(Transaction transaction);
    public Task<Transaction?> GetTransactionByPublicIdAsync(string transactionPublicId);
    public Task<(List<Transaction>, int)> GetTransactionsByPageAsync(string tenantPublicId, 
        string userPublicId, DateTime? startDate, DateTime? endDate,
        int page, int pageSize, string sortBy);
}