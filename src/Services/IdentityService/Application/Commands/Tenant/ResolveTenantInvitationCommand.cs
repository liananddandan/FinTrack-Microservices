using IdentityService.Application.Common.DTOs;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Commands.Tenant;

public record ResolveTenantInvitationCommand(
    string InvitationPublicId,
    string InvitationVersion
) : IRequest<ServiceResult<ResolveTenantInvitationResult>>;