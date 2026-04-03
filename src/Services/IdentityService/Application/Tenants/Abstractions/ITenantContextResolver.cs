using IdentityService.Application.Tenants.Dtos;

namespace IdentityService.Application.Tenants.Abstractions;

public interface ITenantContextResolver
{
    Task<TenantRequestContext?> ResolveAsync(
        string host,
        CancellationToken cancellationToken = default);
}