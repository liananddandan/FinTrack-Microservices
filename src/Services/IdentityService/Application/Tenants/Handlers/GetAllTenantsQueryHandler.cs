using IdentityService.Application.Tenants.Abstractions;
using IdentityService.Application.Tenants.Dtos;
using IdentityService.Application.Tenants.Queries;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Handlers;

public class GetAllTenantsQueryHandler(
    ITenantDirectoryService tenantDirectoryService)
    : IRequestHandler<GetAllTenantsQuery, ServiceResult<IReadOnlyList<TenantSummaryDto>>>
{
    public async Task<ServiceResult<IReadOnlyList<TenantSummaryDto>>> Handle(
        GetAllTenantsQuery request,
        CancellationToken cancellationToken)
    {
        return await tenantDirectoryService.GetAllTenantsAsync(cancellationToken);
    }
}