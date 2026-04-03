using PlatformService.Application.Tenants.Abstractions;
using PlatformService.Application.Tenants.Dtos;
using SharedKernel.Common.Results;

namespace PlatformService.Application.Tenants.Services;

public class PlatformTenantService(
    IIdentityTenantDirectoryClient identityTenantDirectoryClient)
    : IPlatformTenantService
{
    public async Task<ServiceResult<IReadOnlyList<TenantSummaryDto>>> GetAllTenantsAsync(
        CancellationToken cancellationToken = default)
    {
        var tenants = await identityTenantDirectoryClient.GetAllTenantsAsync(cancellationToken);

        return ServiceResult<IReadOnlyList<TenantSummaryDto>>.Ok(
            tenants,
            "Platform.Tenant.GetAllSuccess",
            "Tenants retrieved successfully.");
    }
}