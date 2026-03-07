using IdentityService.Application.Common.DTOs;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Commands.Tenant;

public record GetTenantInvitationsCommand(
    string TenantPublicId
) : IRequest<ServiceResult<List<TenantInvitationDto>>>;