using IdentityService.Application.Commands.Tenant;
using IdentityService.Application.Services.Interfaces;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.CommandHandlers.Tenant;

public class AcceptTenantInvitationCommandHandler(
    ITenantInvitationService tenantInvitationService)
    : IRequestHandler<AcceptTenantInvitationCommand, ServiceResult<bool>>
{
    public async Task<ServiceResult<bool>> Handle(
        AcceptTenantInvitationCommand request,
        CancellationToken cancellationToken)
    {
        return await tenantInvitationService.AcceptInvitationAsync(
            request.InvitationPublicId,
            request.InvitationVersion,
            cancellationToken);
    }
}