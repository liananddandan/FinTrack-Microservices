using IdentityService.Application.Tenants.Dtos;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Abstractions;

public interface ITenantDirectoryService
{
    Task<ServiceResult<IReadOnlyList<TenantSummaryDto>>> GetAllTenantsAsync(
        CancellationToken cancellationToken = default);
}