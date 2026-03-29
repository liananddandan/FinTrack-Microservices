using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Tenants.Abstractions;
using IdentityService.Application.Tenants.Commands;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Handlers;

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