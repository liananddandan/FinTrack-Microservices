using IdentityService.Application.Abstractions;
using IdentityService.Application.Commands.Tenant;
using MediatR;
using SharedKernel.Common.Constants;
using SharedKernel.Common.Results;
using StackExchange.Redis;

namespace IdentityService.Application.CommandHandlers.Tenant;

public class RemoveTenantMemberCommandHandler(
    ITenantService tenantService
) : IRequestHandler<RemoveTenantMemberCommand, ServiceResult<bool>>
{
    public async Task<ServiceResult<bool>> Handle(
        RemoveTenantMemberCommand request,
        CancellationToken cancellationToken)
    {
        return await tenantService.RemoveTenantMemberAsync(
            request.TenantPublicId,
            request.MembershipPublicId,
            request.OperatorUserPublicId,
            cancellationToken);
    }
}