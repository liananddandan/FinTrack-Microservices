namespace PlatformService.Application.Tenants.Abstractions;

using PlatformService.Domain.Entities;
using PlatformService.Domain.Enums;

public interface ITenantDomainMappingRepository
{
    Task<IReadOnlyList<TenantDomainMapping>> GetByTenantPublicIdAsync(
        Guid tenantPublicId,
        CancellationToken cancellationToken = default);

    Task<TenantDomainMapping?> GetByPublicIdAsync(
        Guid publicId,
        CancellationToken cancellationToken = default);

    Task<TenantDomainMapping?> GetByHostAsync(
        string host,
        CancellationToken cancellationToken = default);

    Task<TenantDomainMapping?> GetPrimaryByTenantAndTypeAsync(
        Guid tenantPublicId,
        TenantDomainType domainType,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        TenantDomainMapping mapping,
        CancellationToken cancellationToken = default);

    void Update(TenantDomainMapping mapping);

    void Remove(TenantDomainMapping mapping);
}