using Microsoft.EntityFrameworkCore;
using PlatformService.Application.Tenants.Abstractions;
using PlatformService.Domain.Entities;
using PlatformService.Domain.Enums;

namespace PlatformService.Infrastructure.Persistence.Repositories;

public class TenantDomainMappingRepository(
    PlatformDbContext dbContext)
    : ITenantDomainMappingRepository
{
    public async Task<IReadOnlyList<TenantDomainMapping>> GetByTenantPublicIdAsync(
        Guid tenantPublicId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.TenantDomainMappings
            .AsNoTracking()
            .Where(x => x.TenantPublicId == tenantPublicId)
            .OrderBy(x => x.DomainType)
            .ThenByDescending(x => x.IsPrimary)
            .ThenBy(x => x.Host)
            .ToListAsync(cancellationToken);
    }

    public async Task<TenantDomainMapping?> GetByPublicIdAsync(
        Guid publicId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.TenantDomainMappings
            .FirstOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);
    }

    public async Task<TenantDomainMapping?> GetByHostAsync(
        string host,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.TenantDomainMappings
            .FirstOrDefaultAsync(x => x.Host == host, cancellationToken);
    }

    public async Task<TenantDomainMapping?> GetPrimaryByTenantAndTypeAsync(
        Guid tenantPublicId,
        TenantDomainType domainType,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.TenantDomainMappings
            .FirstOrDefaultAsync(
                x => x.TenantPublicId == tenantPublicId &&
                     x.DomainType == domainType &&
                     x.IsPrimary,
                cancellationToken);
    }

    public async Task AddAsync(
        TenantDomainMapping mapping,
        CancellationToken cancellationToken = default)
    {
        await dbContext.TenantDomainMappings.AddAsync(mapping, cancellationToken);
    }

    public void Update(TenantDomainMapping mapping)
    {
        dbContext.TenantDomainMappings.Update(mapping);
    }

    public void Remove(TenantDomainMapping mapping)
    {
        dbContext.TenantDomainMappings.Remove(mapping);
    }
}