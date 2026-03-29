using IdentityService.Application.Tenants.Abstractions;
using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Repositories;

public class TenantInvitationRepo(ApplicationIdentityDbContext dbContext) : ITenantInvitationRepo
{
    public async Task AddAsync(TenantInvitation invitation, CancellationToken cancellationToken = default)
    {
        await dbContext.TenantInvitations.AddAsync(invitation, cancellationToken);
    }

    public async Task<TenantInvitation?> GetByPublicIdAsync(
        Guid publicId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.TenantInvitations
            .Include(x => x.Tenant)
            .Include(x => x.CreatedByUser)
            .FirstOrDefaultAsync(
                x => x.PublicId == publicId,
                cancellationToken);
    }

    public async Task<List<TenantInvitation>> GetByTenantPublicIdAsync(
        string tenantPublicId,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(tenantPublicId, out var parsedTenantPublicId))
        {
            return new List<TenantInvitation>();
        }

        return await dbContext.TenantInvitations
            .Include(x => x.Tenant)
            .Include(x => x.CreatedByUser)
            .Where(x => x.Tenant.PublicId == parsedTenantPublicId && !x.Tenant.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<TenantInvitation?> FindByEmailAsync(string email)
    {
        return await dbContext.TenantInvitations.Where(ti => ti.Email == email).FirstOrDefaultAsync();
    }

    public Task UpdateAsync(TenantInvitation invitation)
    {
        dbContext.TenantInvitations.Update(invitation);
        return Task.CompletedTask;
    }
}