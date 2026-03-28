using IdentityService.Application.Abstractions;
using IdentityService.Application.Commands.Tenant;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.CommandHandlers.Tenant;

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