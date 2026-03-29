using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Tenants.Abstractions;
using IdentityService.Application.Tenants.Commands;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Handlers;

public class ResolveTenantInvitationCommandHandler(
    ITenantInvitationService tenantInvitationService)
    : IRequestHandler<ResolveTenantInvitationCommand, ServiceResult<ResolveTenantInvitationDto>>
{
    public async Task<ServiceResult<ResolveTenantInvitationDto>> Handle(
        ResolveTenantInvitationCommand request,
        CancellationToken cancellationToken)
    {
        return await tenantInvitationService.ResolveInvitationAsync(
            request.InvitationPublicId,
            request.InvitationVersion,
            cancellationToken);
    }
}