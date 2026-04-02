using IdentityService.Application.Tenants.Abstractions;
using IdentityService.Application.Tenants.Dtos;
using IdentityService.Application.Tenants.Queries;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Handlers;

public class GetTenantContextQueryHandler(
    ITenantContextService tenantContextService)
    : IRequestHandler<GetTenantContextQuery, ServiceResult<TenantContextDto?>>
{
    public async Task<ServiceResult<TenantContextDto?>> Handle(
        GetTenantContextQuery request,
        CancellationToken cancellationToken)
    {
        var result = await tenantContextService.GetTenantContextAsync(
            request.Host,
            cancellationToken);

        return ServiceResult<TenantContextDto?>.Ok(
            result,
            "Identity.TenantContext.GetSuccess",
            "Tenant context retrieved successfully.");
    }
}