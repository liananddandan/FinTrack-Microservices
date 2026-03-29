using IdentityService.Application.Common.DTOs;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Commands;

public record GetTenantMembersCommand(
    string TenantPublicId
) : IRequest<ServiceResult<List<TenantMemberDto>>>;