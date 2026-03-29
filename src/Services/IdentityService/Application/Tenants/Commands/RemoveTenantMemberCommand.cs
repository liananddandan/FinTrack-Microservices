using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Commands;

public record RemoveTenantMemberCommand(
    string MembershipPublicId,
    string TenantPublicId,
    string OperatorUserPublicId
) : IRequest<ServiceResult<bool>>;