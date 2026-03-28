using GatewayService.Application.DTOs;

namespace GatewayService.Application.Services.Interfaces;

public interface IDevSeedOrchestrator
{
    Task<DevSeedResult> SeedAsync(CancellationToken cancellationToken = default);
}
