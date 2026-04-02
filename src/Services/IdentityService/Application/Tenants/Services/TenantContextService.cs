using IdentityService.Application.Tenants.Abstractions;
using IdentityService.Application.Tenants.Dtos;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Application.Tenants.Services;


public class TenantContextService(
    ITenantDomainProjectionRepository tenantDomainProjectionRepository,
    ApplicationIdentityDbContext dbContext)
    : ITenantContextService
{
    public async Task<TenantContextDto?> GetTenantContextAsync(
        string host,
        CancellationToken cancellationToken = default)
    {
        var normalizedHost = NormalizeHost(host);

        if (string.IsNullOrWhiteSpace(normalizedHost))
        {
            return null;
        }

        var projection = await tenantDomainProjectionRepository.GetByHostAsync(
            normalizedHost,
            cancellationToken);

        if (projection is null || !projection.IsActive)
        {
            return null;
        }

        var tenant = await dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.PublicId == projection.TenantPublicId &&
                     x.IsActive &&
                     !x.IsDeleted,
                cancellationToken);

        if (tenant is null)
        {
            return null;
        }

        return new TenantContextDto
        {
            TenantPublicId = tenant.PublicId.ToString(),
            TenantName = tenant.Name,
            Host = projection.Host,
            IsActive = tenant.IsActive,
            LogoUrl = tenant.LogoUrl,
            ThemeColor = tenant.ThemeColor
        };
    }

    private static string NormalizeHost(string host)
    {
        var normalized = host.Trim().ToLowerInvariant();

        var colonIndex = normalized.IndexOf(':');
        if (colonIndex >= 0)
        {
            normalized = normalized[..colonIndex];
        }

        return normalized;
    }
}