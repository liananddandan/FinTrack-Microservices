using IdentityService.Application.Common.DTOs;
using MediatR;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Commands.Tenant;

public record GetTenantMembersCommand(
    string TenantPublicId
) : IRequest<ServiceResult<List<TenantMemberDto>>>;