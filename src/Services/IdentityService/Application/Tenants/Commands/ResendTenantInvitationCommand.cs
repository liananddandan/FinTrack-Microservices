using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Commands;

public record ResendTenantInvitationCommand(
    string TenantPublicId,
    string InvitationPublicId
) : IRequest<ServiceResult<bool>>;