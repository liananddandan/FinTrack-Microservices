using IdentityService.Commands;
using IdentityService.Common.DTOs;
using IdentityService.Domain.Entities;
using IdentityService.Services.Interfaces;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.CommandHandlers;

public class GetUsersInTenantCommandHandler(ITenantService tenantService)
    : IRequestHandler<GetUsersInTenantCommand, ServiceResult<IEnumerable<UserInfoDto>>>
{
    public async Task<ServiceResult<IEnumerable<UserInfoDto>>> Handle(GetUsersInTenantCommand request, CancellationToken cancellationToken)
    {
        return await tenantService.GetUsersForTenantAsync(request.AdminPublicId,
            request.TenantPublicId, request.AdminRoleInTenant, cancellationToken);
    }
}