using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Commands.Tenant;

public record RemoveTenantMemberCommand(
    string MembershipPublicId,
    string TenantPublicId,
    string OperatorUserPublicId
) : IRequest<ServiceResult<bool>>;