using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Tenants.Abstractions;
using IdentityService.Application.Tenants.Commands;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Handlers;

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