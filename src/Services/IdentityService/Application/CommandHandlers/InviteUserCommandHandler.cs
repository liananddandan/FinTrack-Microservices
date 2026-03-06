using IdentityService.Application.Commands;
using IdentityService.Application.Services.Interfaces;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.CommandHandlers;

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