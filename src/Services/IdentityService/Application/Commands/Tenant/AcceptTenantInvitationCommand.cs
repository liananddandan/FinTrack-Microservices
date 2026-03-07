using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Commands.Tenant;

public record AcceptTenantInvitationCommand(
    string InvitationPublicId,
    string InvitationVersion
) : IRequest<ServiceResult<bool>>;