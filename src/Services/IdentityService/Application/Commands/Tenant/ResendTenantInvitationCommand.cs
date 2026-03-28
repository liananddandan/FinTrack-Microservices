using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Commands.Tenant;

public record ResendTenantInvitationCommand(
    string TenantPublicId,
    string InvitationPublicId
) : IRequest<ServiceResult<bool>>;