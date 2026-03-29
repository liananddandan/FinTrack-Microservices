using GatewayService.Application.Dev.Dtos;

namespace GatewayService.Application.Dev.Abstractions;

public interface IDevSeedOrchestrator
{
    Task<DevSeedResult> SeedAsync(CancellationToken cancellationToken = default);
}
