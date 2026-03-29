using IdentityService.Application.Tenants.Abstractions;
using IdentityService.Application.Tenants.Commands;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Handlers;

public class ChangeTenantMemberRoleCommandHandler(
    ITenantService tenantService
) : IRequestHandler<ChangeTenantMemberRoleCommand, ServiceResult<bool>>
{
    public async Task<ServiceResult<bool>> Handle(
        ChangeTenantMemberRoleCommand request,
        CancellationToken cancellationToken)
    {
        return await tenantService.ChangeTenantMemberRoleAsync(
            request.TenantPublicId,
            request.MembershipPublicId,
            request.OperatorUserPublicId,
            request.Role,
            cancellationToken);
    }
}