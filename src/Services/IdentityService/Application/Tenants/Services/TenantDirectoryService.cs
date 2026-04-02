using IdentityService.Application.Tenants.Abstractions;
using IdentityService.Application.Tenants.Dtos;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Services;

public class TenantDirectoryService(
    ITenantRepository tenantRepository)
    : ITenantDirectoryService
{
    public async Task<ServiceResult<IReadOnlyList<TenantSummaryDto>>> GetAllTenantsAsync(
        CancellationToken cancellationToken = default)
    {
        var tenants = await tenantRepository.GetAllAsync(cancellationToken);

        var result = tenants
            .OrderBy(x => x.Name)
            .Select(x => new TenantSummaryDto
            {
                TenantPublicId = x.PublicId.ToString(),
                TenantName = x.Name,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .ToList();

        return ServiceResult<IReadOnlyList<TenantSummaryDto>>.Ok(
            result,
            "Identity.Tenant.GetAllSuccess",
            "Tenants retrieved successfully.");
    }
}