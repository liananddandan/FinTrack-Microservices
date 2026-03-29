using SharedKernel.Contracts.Dev;

namespace IdentityService.Application.Dev.Abstractions;

public interface IDevSeedService
{
    Task<DevIdentitySeedResult> SeedIdentityAsync(
        DevIdentitySeedRequest request,
        CancellationToken cancellationToken = default);}
