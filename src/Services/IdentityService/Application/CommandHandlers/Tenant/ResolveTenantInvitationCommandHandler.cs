using IdentityService.Application.Commands.Tenant;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Services.Interfaces;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.CommandHandlers.Tenant;

public class ResolveTenantInvitationCommandHandler(
    ITenantInvitationService tenantInvitationService)
    : IRequestHandler<ResolveTenantInvitationCommand, ServiceResult<ResolveTenantInvitationResult>>
{
    public async Task<ServiceResult<ResolveTenantInvitationResult>> Handle(
        ResolveTenantInvitationCommand request,
        CancellationToken cancellationToken)
    {
        return await tenantInvitationService.ResolveInvitationAsync(
            request.InvitationPublicId,
            request.InvitationVersion,
            cancellationToken);
    }
}