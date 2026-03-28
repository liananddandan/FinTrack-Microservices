using IdentityService.Application.Abstractions;
using IdentityService.Application.Commands;
using IdentityService.Application.Commands.Tenant;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.CommandHandlers.Tenant;

public class CreateTenantInvitationCommandHandler(
    ITenantInvitationService invitationService
) : IRequestHandler<CreateTenantInvitationCommand, ServiceResult<bool>>
{
    public async Task<ServiceResult<bool>> Handle(
        CreateTenantInvitationCommand request,
        CancellationToken cancellationToken)
    {
        return await invitationService.CreateInvitationAsync(
            request.TenantPublicId,
            request.Email,
            request.Role,
            request.InvitedByUserPublicId,
            cancellationToken);
    }
}