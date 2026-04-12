using IdentityService.Application.Tenants.Abstractions;
using IdentityService.Application.Tenants.Dtos;
using IdentityService.Application.Tenants.Queries;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Handlers;

public class GetTenantStripeConnectStatusQueryHandler(
    ITenantStripeConnectService tenantStripeConnectService)
    : IRequestHandler<GetTenantStripeConnectStatusQuery, ServiceResult<TenantStripeConnectStatusDto>>
{
    public async Task<ServiceResult<TenantStripeConnectStatusDto>> Handle(
        GetTenantStripeConnectStatusQuery request,
        CancellationToken cancellationToken)
    {
        return await tenantStripeConnectService.GetStatusAsync(
            request.TenantPublicId.ToString(),
            cancellationToken);
    }
}