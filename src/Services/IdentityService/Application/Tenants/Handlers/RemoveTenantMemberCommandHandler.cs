using IdentityService.Application.Tenants.Abstractions;
using IdentityService.Application.Tenants.Commands;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Handlers;

public class RemoveTenantMemberCommandHandler(
    ITenantService tenantService
) : IRequestHandler<RemoveTenantMemberCommand, ServiceResult<bool>>
{
    public async Task<ServiceResult<bool>> Handle(
        RemoveTenantMemberCommand request,
        CancellationToken cancellationToken)
    {
        return await tenantService.RemoveTenantMemberAsync(
            request.TenantPublicId,
            request.MembershipPublicId,
            request.OperatorUserPublicId,
            cancellationToken);
    }
}