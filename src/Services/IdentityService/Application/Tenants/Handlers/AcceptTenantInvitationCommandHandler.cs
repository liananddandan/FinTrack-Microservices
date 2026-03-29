using IdentityService.Application.Tenants.Abstractions;
using IdentityService.Application.Tenants.Commands;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Handlers;

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