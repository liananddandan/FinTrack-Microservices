using SharedKernel.Contracts.Dev;

namespace TransactionService.Application.Dev.Abstractions;

public interface IDevSeedService
{
    Task<DevTransactionSeedResult> SeedMenuAndOrdersAsync(
        DevTransactionSeedRequest request,
        CancellationToken cancellationToken = default);
}
