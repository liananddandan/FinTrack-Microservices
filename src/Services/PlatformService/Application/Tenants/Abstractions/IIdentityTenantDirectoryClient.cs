using PlatformService.Application.Tenants.Dtos;

namespace PlatformService.Application.Tenants.Abstractions;

public interface IIdentityTenantDirectoryClient
{
    Task<IReadOnlyList<TenantSummaryDto>> GetAllTenantsAsync(
        CancellationToken cancellationToken = default);
}