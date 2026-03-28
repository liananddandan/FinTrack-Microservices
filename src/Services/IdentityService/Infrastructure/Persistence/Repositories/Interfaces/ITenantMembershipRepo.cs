using IdentityService.Domain.Entities;

namespace IdentityService.Infrastructure.Persistence.Repositories.Interfaces;

public interface ITenantMembershipRepo
{
    Task AddMembershipAsync(TenantMembership membership, CancellationToken cancellationToken = default);
    Task<List<TenantMembership>> GetMembershipsByTenantPublicIdAsync(
        string tenantPublicId,
        CancellationToken cancellationToken = default);
    Task<TenantMembership?> GetMembershipAsync(
        long tenantId,
        long userId,
        CancellationToken cancellationToken = default);

    Task<TenantMembership?> GetByPublicIdAsync(
        string membershipPublicId,
        CancellationToken cancellationToken = default);

    Task<TenantMembership?> GetAnyMembershipAsync(
        long tenantId,
        long userId,
        CancellationToken cancellationToken = default);
    
    Task<int> CountActiveAdminsAsync(
        long tenantId,
        CancellationToken cancellationToken = default);
    
    
}