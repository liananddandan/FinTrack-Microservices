using IdentityService.Commands;
using IdentityService.Services.Interfaces;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.CommandHandlers;

public class InviteUserCommandHandler (
    ITenantService tenantService): IRequestHandler<InviteUserCommand, ServiceResult<bool>>
{
    public async Task<ServiceResult<bool>> Handle(InviteUserCommand request, CancellationToken cancellationToken)
    {
        return await tenantService.InviteUserForTenantAsync(
            request.AdminUserPublicId,
            request.TenantPublicId,
            request.AdminRoleInTenant,
            request.Emails,
            cancellationToken
        );
    }
}