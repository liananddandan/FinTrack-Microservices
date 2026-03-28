using TransactionService.Api.Dev.Contracts;

namespace TransactionService.Application.Dev.Abstractions;

public interface IDevSeedService
{
    Task<DevTransactionSeedResult> SeedTransactionsAsync(
        DevTransactionSeedRequest request,
        CancellationToken cancellationToken = default);
}
