using IdentityService.Application.Tenants.Abstractions;
using IdentityService.Application.Tenants.Dtos;

namespace IdentityService.Application.Tenants.Services;


public class TenantContextResolver(
    ITenantDomainProjectionRepository tenantDomainProjectionRepository)
    : ITenantContextResolver
{
    // TODO:
    // Optimization note:
    // Current implementation resolves tenant domain from database on each request.
    // Future optimization:
    // 1. add IMemoryCache for Host -> TenantRequestContext
    // 2. invalidate cache when tenant domain projection is updated
    // 3. upgrade to Redis if multiple Identity instances are deployed
    public async Task<TenantRequestContext?> ResolveAsync(
        string host,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            return null;
        }

        var normalizedHost = host.Trim().ToLowerInvariant();

        var projection = await tenantDomainProjectionRepository.GetByHostAsync(
            normalizedHost,
            cancellationToken);

        if (projection is null)
        {
            return null;
        }

        if (!projection.IsActive)
        {
            return null;
        }

        return new TenantRequestContext
        {
            TenantPublicId = projection.TenantPublicId,
            Host = projection.Host,
            DomainType = projection.DomainType,
            IsPrimary = projection.IsPrimary,
            IsActive = projection.IsActive
        };
    }
}