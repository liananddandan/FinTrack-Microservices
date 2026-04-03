

using IdentityService.Application.Tenants.Abstractions;
using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Repositories;

public class TenantDomainProjectionRepository(
    ApplicationIdentityDbContext dbContext)
    : ITenantDomainProjectionRepository
{
    public async Task<TenantDomainProjection?> GetByDomainPublicIdAsync(
        Guid domainPublicId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.TenantDomainProjections
            .FirstOrDefaultAsync(x => x.DomainPublicId == domainPublicId, cancellationToken);
    }

    public async Task<TenantDomainProjection?> GetByHostAsync(
        string host,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.TenantDomainProjections
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Host == host, cancellationToken);
    }

    public async Task AddAsync(
        TenantDomainProjection projection,
        CancellationToken cancellationToken = default)
    {
        await dbContext.TenantDomainProjections.AddAsync(projection, cancellationToken);
    }

    public void Update(TenantDomainProjection projection)
    {
        dbContext.TenantDomainProjections.Update(projection);
    }

    public void Remove(TenantDomainProjection projection)
    {
        dbContext.TenantDomainProjections.Remove(projection);
    }
}