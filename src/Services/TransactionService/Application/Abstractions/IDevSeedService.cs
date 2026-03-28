using TransactionService.Application.DTOs;

namespace TransactionService.Application.Abstractions;

public interface IDevSeedService
{
    Task<DevTransactionSeedResult> SeedTransactionsAsync(
        DevTransactionSeedRequest request,
        CancellationToken cancellationToken = default);
}
