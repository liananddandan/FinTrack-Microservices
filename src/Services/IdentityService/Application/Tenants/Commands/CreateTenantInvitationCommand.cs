using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Commands;

public record CreateTenantInvitationCommand(
    string TenantPublicId,
    string Email,
    string Role,
    string InvitedByUserPublicId
) : IRequest<ServiceResult<bool>>;