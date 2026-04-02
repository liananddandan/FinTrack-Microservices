using MediatR;
using PlatformService.Application.Tenants.Abstractions;
using PlatformService.Application.Tenants.Dtos;
using PlatformService.Application.Tenants.Queries;
using SharedKernel.Common.Results;

namespace PlatformService.Application.Tenants.Handlers;

public class GetAllPlatformTenantsQueryHandler(
    IPlatformTenantService platformTenantService)
    : IRequestHandler<GetAllPlatformTenantsQuery, ServiceResult<IReadOnlyList<TenantSummaryDto>>>
{
    public async Task<ServiceResult<IReadOnlyList<TenantSummaryDto>>> Handle(
        GetAllPlatformTenantsQuery request,
        CancellationToken cancellationToken)
    {
        return await platformTenantService.GetAllTenantsAsync(cancellationToken);
    }
}