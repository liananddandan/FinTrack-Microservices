using IdentityService.Application.Tenants.Abstractions;
using IdentityService.Application.Tenants.Commands;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Handlers;

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