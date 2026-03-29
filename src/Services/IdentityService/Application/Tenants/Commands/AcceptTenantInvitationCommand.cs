using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Commands;

public record AcceptTenantInvitationCommand(
    string InvitationPublicId,
    string InvitationVersion
) : IRequest<ServiceResult<bool>>;