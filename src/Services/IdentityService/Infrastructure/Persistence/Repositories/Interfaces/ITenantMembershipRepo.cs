using IdentityService.Domain.Entities;

namespace IdentityService.Infrastructure.Persistence.Repositories.Interfaces;

public interface ITenantMembershipRepo
{
    Task AddMembershipAsync(TenantMembership membership, CancellationToken cancellationToken = default);
    Task<List<TenantMembership>> GetMembershipsByTenantPublicIdAsync(
        string tenantPublicId,
        CancellationToken cancellationToken = default);
}