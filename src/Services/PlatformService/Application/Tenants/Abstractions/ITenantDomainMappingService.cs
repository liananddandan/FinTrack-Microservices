
using PlatformService.Application.Tenants.Commands;
using PlatformService.Application.Tenants.Dtos;
using SharedKernel.Common.Results;

namespace PlatformService.Application.Tenants.Abstractions;

public interface ITenantDomainMappingService
{
    Task<ServiceResult<IReadOnlyList<TenantDomainMappingDto>>> GetByTenantAsync(
        Guid tenantPublicId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<TenantDomainMappingDto>> CreateAsync(
        CreateTenantDomainMappingCommand command,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<TenantDomainMappingDto>> UpdateAsync(
        UpdateTenantDomainMappingCommand command,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> DeleteAsync(
        DeleteTenantDomainMappingCommand command,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<TenantDomainMappingDto>> SetActiveAsync(
        SetTenantDomainMappingActiveCommand command,
        CancellationToken cancellationToken = default);
}