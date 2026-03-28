using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Repositories;

public class TenantRepo(ApplicationIdentityDbContext dbContext) : ITenantRepo
{
    public async Task AddTenantAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await dbContext.Tenants.AddAsync(tenant, cancellationToken);
    }

    public async Task<Tenant?> GetTenantByPublicIdAsync(string publicId, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(publicId, out var parsedPublicId))
        {
            return null;
        }

        return await dbContext.Tenants
            .FirstOrDefaultAsync(t => t.PublicId == parsedPublicId
                , cancellationToken);
    }

    public async Task<Tenant?> GetTenantByNameAsync(string tenantName, CancellationToken cancellationToken = default)
    {
        tenantName = tenantName.Trim();

        return await dbContext.Tenants
            .FirstOrDefaultAsync(t => t.Name == tenantName, cancellationToken);
    }

    public async Task<bool> IsTenantNameExistsAsync(string tenantName, CancellationToken cancellationToken = default)
    {
        return await dbContext.Tenants
            .AnyAsync(t => t.Name == tenantName && !t.IsDeleted, cancellationToken);    }
}
