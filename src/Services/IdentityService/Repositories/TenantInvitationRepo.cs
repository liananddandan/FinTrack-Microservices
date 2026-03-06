using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Repositories;

public class TenantInvitationRepo(ApplicationIdentityDbContext dbContext) : ITenantInvitationRepo
{
    public Task AddAsync(TenantInvitation invitation)
    {
        dbContext.TenantInvitations.Add(invitation);
        return Task.CompletedTask;
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