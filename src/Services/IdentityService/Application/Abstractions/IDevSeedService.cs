using IdentityService.Application.DTOs;

namespace IdentityService.Application.Abstractions;

public interface IDevSeedService
{
    Task<DevIdentitySeedResult> SeedIdentityAsync(CancellationToken cancellationToken = default);
}
