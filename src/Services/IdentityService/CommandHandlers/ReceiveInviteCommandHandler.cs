using IdentityService.Commands;
using IdentityService.Services.Interfaces;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.CommandHandlers;

public class ReceiveInviteCommandHandler(
    ITenantService tenantService) : IRequestHandler<ReceiveInviteCommand, ServiceResult<bool>>
{
    public Task<ServiceResult<bool>> Handle(ReceiveInviteCommand request, CancellationToken cancellationToken)
    {
        return tenantService.ReceiveInviteForTenantAsync(request.InvitationPublicId, cancellationToken);
    }
}