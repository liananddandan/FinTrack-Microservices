using IdentityService.Domain.Entities;

namespace IdentityService.Repositories.Interfaces;

public interface ITenantRepo
{
    Task AddTenantAsync(Tenant tenant, CancellationToken cancellationToken = default);
}