using TransactionService.Application.DTOs;

namespace TransactionService.Application.Services.Interfaces;

public interface IDevSeedService
{
    Task<DevTransactionSeedResult> SeedTransactionsAsync(
        DevTransactionSeedRequest request,
        CancellationToken cancellationToken = default);
}
