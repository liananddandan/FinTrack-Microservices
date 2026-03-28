using IdentityService.Application.Commands.Tenant;
using IdentityService.Application.Services.Interfaces;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.CommandHandlers.Tenant;


public class ResendTenantInvitationCommandHandler(
    ITenantInvitationService tenantInvitationService)
    : IRequestHandler<ResendTenantInvitationCommand, ServiceResult<bool>>
{
    public async Task<ServiceResult<bool>> Handle(
        ResendTenantInvitationCommand request,
        CancellationToken cancellationToken)
    {
        return await tenantInvitationService.ResendInvitationAsync(
            request.TenantPublicId,
            request.InvitationPublicId,
            cancellationToken);
    }
}