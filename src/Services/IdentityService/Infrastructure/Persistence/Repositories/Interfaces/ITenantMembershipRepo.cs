using IdentityService.Domain.Entities;

namespace IdentityService.Infrastructure.Persistence.Repositories.Interfaces;

public interface ITenantMembershipRepo
{
    Task AddMembershipAsync(TenantMembership membership, CancellationToken cancellationToken = default);
}