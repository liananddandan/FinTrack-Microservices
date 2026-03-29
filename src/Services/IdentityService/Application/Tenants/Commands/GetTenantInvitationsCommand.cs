using IdentityService.Application.Common.DTOs;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Commands;

public record GetTenantInvitationsCommand(
    string TenantPublicId
) : IRequest<ServiceResult<List<TenantInvitationDto>>>;