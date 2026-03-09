using IdentityService.Application.DTOs;

namespace IdentityService.Application.Services.Interfaces;

public interface IDevSeedService
{
    Task<DevIdentitySeedResult> SeedIdentityAsync(CancellationToken cancellationToken = default);
}
