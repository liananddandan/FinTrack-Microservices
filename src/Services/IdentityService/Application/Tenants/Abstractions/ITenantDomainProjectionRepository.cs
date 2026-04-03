using IdentityService.Domain.Entities;

namespace IdentityService.Application.Tenants.Abstractions;

public interface ITenantDomainProjectionRepository
{
    Task<TenantDomainProjection?> GetByDomainPublicIdAsync(
        Guid domainPublicId,
        CancellationToken cancellationToken = default);

    Task<TenantDomainProjection?> GetByHostAsync(
        string host,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        TenantDomainProjection projection,
        CancellationToken cancellationToken = default);

    void Update(TenantDomainProjection projection);

    void Remove(TenantDomainProjection projection);
}