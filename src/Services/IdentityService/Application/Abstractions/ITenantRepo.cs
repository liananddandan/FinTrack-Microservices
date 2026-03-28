using IdentityService.Domain.Entities;

namespace IdentityService.Application.Abstractions;

public interface ITenantRepo
{
    Task AddTenantAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task<Tenant?> GetTenantByPublicIdAsync(string publicId, CancellationToken cancellationToken = default);
    Task<Tenant?> GetTenantByNameAsync(string tenantName, CancellationToken cancellationToken = default);
    Task<bool> IsTenantNameExistsAsync(string tenantName, CancellationToken cancellationToken = default);
}
