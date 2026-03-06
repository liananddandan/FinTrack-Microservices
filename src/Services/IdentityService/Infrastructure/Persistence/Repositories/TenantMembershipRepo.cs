using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence.Repositories.Interfaces;

namespace IdentityService.Infrastructure.Persistence.Repositories;

public class TenantMembershipRepo(ApplicationIdentityDbContext dbContext) : ITenantMembershipRepo
{
    public async Task AddMembershipAsync(TenantMembership membership, CancellationToken cancellationToken = default)
    {
        await dbContext.TenantMemberships.AddAsync(membership, cancellationToken);
    }
}