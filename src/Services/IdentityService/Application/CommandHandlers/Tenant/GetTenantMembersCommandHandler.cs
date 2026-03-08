using IdentityService.Application.Commands;
using IdentityService.Application.Commands.Tenant;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Services.Interfaces;
using MediatR;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;

namespace IdentityService.Application.CommandHandlers.Tenant;

public class GetTenantMembersCommandHandler(ITenantService tenantService)
    : IRequestHandler<GetTenantMembersCommand, ServiceResult<List<TenantMemberDto>>>
{
    public async Task<ServiceResult<List<TenantMemberDto>>> Handle(
        GetTenantMembersCommand request,
        CancellationToken cancellationToken)
    {
        return await tenantService.GetTenantMembersAsync(
            request.TenantPublicId,
            cancellationToken);
    }
}