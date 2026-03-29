using IdentityService.Application.Tenants.Abstractions;
using IdentityService.Application.Tenants.Commands;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Handlers;


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