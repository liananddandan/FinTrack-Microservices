using IdentityService.Application.Tenants.Dtos;

namespace IdentityService.Application.Tenants.Abstractions;

public interface ITenantContextService
{
    Task<TenantContextDto?> GetTenantContextAsync(
        string host,
        CancellationToken cancellationToken = default);
}