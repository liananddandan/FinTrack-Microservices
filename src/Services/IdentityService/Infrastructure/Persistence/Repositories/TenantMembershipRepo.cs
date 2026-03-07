using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Repositories;

public class TenantMembershipRepo(ApplicationIdentityDbContext dbContext) : ITenantMembershipRepo
{
    public async Task AddMembershipAsync(TenantMembership membership, CancellationToken cancellationToken = default)
    {
        await dbContext.TenantMemberships.AddAsync(membership, cancellationToken);
    }
    
    public async Task<List<TenantMembership>> GetMembershipsByTenantPublicIdAsync(
        string tenantPublicId,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(tenantPublicId, out var parsedTenantPublicId))
        {
            return new List<TenantMembership>();
        }

        return await dbContext.TenantMemberships
            .Include(m => m.User)
            .Include(m => m.Tenant)
            .Where(m => m.Tenant.PublicId == parsedTenantPublicId && !m.Tenant.IsDeleted)
            .OrderBy(m => m.JoinedAt)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<TenantMembership?> GetMembershipAsync(
        long tenantId,
        long userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.TenantMemberships
            .FirstOrDefaultAsync(
                m => m.TenantId == tenantId 
                     && m.UserId == userId
                     && m.IsActive,
                cancellationToken);
    }

    public async Task<TenantMembership?> GetByPublicIdAsync(string memberShipPublicId, CancellationToken cancellationToken = default)
    {
        return await dbContext.TenantMemberships
            .Include(x => x.Tenant)
            .Include(x => x.User)
            .FirstOrDefaultAsync(
                x => x.PublicId.ToString() == memberShipPublicId,
                cancellationToken);
        
    }
    
    public async Task<TenantMembership?> GetAnyMembershipAsync(
        long tenantId,
        long userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.TenantMemberships
            .FirstOrDefaultAsync(
                m => m.TenantId == tenantId &&
                     m.UserId == userId,
                cancellationToken);
    }
}