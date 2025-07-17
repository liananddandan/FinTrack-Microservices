using IdentityService.Domain.Entities;

namespace IdentityService.Repositories.Interfaces;

public interface ITenantRepo
{
    Task AddTenantAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task<Tenant?> GetTenantByPublicIdAsync(string publicId, CancellationToken cancellationToken = default);
}