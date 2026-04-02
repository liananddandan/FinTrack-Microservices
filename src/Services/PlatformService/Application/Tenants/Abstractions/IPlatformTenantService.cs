using PlatformService.Application.Tenants.Dtos;
using SharedKernel.Common.Results;

namespace PlatformService.Application.Tenants.Abstractions;

public interface IPlatformTenantService
{
    Task<ServiceResult<IReadOnlyList<TenantSummaryDto>>> GetAllTenantsAsync(
        CancellationToken cancellationToken = default);
}