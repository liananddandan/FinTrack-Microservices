using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Repositories.Interfaces;

namespace IdentityService.Repositories;

public class TenantRepo(ApplicationIdentityDbContext dbContext) : ITenantRepo
{
    public async Task AddTenantAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await dbContext.AddAsync(tenant, cancellationToken);
    }
}