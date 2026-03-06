using IdentityService.Application.Commands;
using IdentityService.Application.Services.Interfaces;
using MediatR;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;

namespace IdentityService.Application.CommandHandlers;

public class GetUsersInTenantCommandHandler(ITenantService tenantService)
    : IRequestHandler<GetUsersInTenantCommand, ServiceResult<IEnumerable<UserInfoDto>>>
{
    public async Task<ServiceResult<IEnumerable<UserInfoDto>>> Handle(GetUsersInTenantCommand request, CancellationToken cancellationToken)
    {
        return await tenantService.GetUsersForTenantAsync(request.AdminPublicId,
            request.TenantPublicId, request.AdminRoleInTenant, cancellationToken);
    }
}