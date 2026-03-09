using GatewayService.DTOs;

namespace GatewayService.Services.Interfaces;

public interface IDevSeedOrchestrator
{
    Task<DevSeedResult> SeedAsync(CancellationToken cancellationToken = default);
}
