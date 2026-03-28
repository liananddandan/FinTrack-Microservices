using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Commands.Tenant;

public record ChangeTenantMemberRoleCommand(
    string TenantPublicId,
    string MembershipPublicId,
    string OperatorUserPublicId,
    string Role
) : IRequest<ServiceResult<bool>>;