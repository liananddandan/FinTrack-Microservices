using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence.Repositories.Interfaces;
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

    public async Task<TenantInvitation?> FindByPublicIdAsync(Guid publicId)
    {
        return await dbContext.TenantInvitations.Where(ti => ti.PublicId == publicId).FirstOrDefaultAsync();
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