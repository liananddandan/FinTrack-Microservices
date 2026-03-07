using IdentityService.Domain.Entities;

namespace IdentityService.Infrastructure.Persistence.Repositories.Interfaces;

public interface ITenantInvitationRepo
{
    Task AddAsync(TenantInvitation invitation, CancellationToken cancellationToken = default);
    Task<TenantInvitation?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken);
    
    Task<TenantInvitation?> FindByPublicIdAsync(Guid id);
    Task<TenantInvitation?> FindByEmailAsync(string email);
    
    Task UpdateAsync(TenantInvitation invitation);
}