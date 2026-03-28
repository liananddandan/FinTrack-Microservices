using IdentityService.Application.Abstractions;
using IdentityService.Application.Commands.Tenant;
using IdentityService.Application.Common.DTOs;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.CommandHandlers.Tenant;

public class GetTenantInvitationsCommandHandler(
    ITenantInvitationService tenantInvitationService)
    : IRequestHandler<GetTenantInvitationsCommand, ServiceResult<List<TenantInvitationDto>>>
{
    public async Task<ServiceResult<List<TenantInvitationDto>>> Handle(
        GetTenantInvitationsCommand request,
        CancellationToken cancellationToken)
    {
        return await tenantInvitationService.GetTenantInvitationsAsync(
            request.TenantPublicId,
            cancellationToken);
    }
}