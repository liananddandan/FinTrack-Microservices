using IdentityService.Application.Common.DTOs;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Commands;

public record ResolveTenantInvitationCommand(
    string InvitationPublicId,
    string InvitationVersion
) : IRequest<ServiceResult<ResolveTenantInvitationDto>>;