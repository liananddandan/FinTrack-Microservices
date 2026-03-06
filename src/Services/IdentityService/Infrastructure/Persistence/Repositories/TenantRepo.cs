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
        return await dbContext.Tenants
            .Where(t => publicId.Equals(t.PublicId.ToString()))
            .FirstOrDefaultAsync(cancellationToken);
    }
}