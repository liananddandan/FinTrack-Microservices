using IdentityService.Domain.Entities;

namespace IdentityService.Infrastructure.Persistence.Repositories.Interfaces;

public interface ITenantInvitationRepo
{
    Task AddAsync(TenantInvitation invitation);
    Task<TenantInvitation?> FindByPublicIdAsync(Guid id);
    Task<TenantInvitation?> FindByEmailAsync(string email);
    
    Task UpdateAsync(TenantInvitation invitation);
}