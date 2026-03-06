using MediatR;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Commands;

public record GetUsersInTenantCommand(
    string AdminPublicId,
    string TenantPublicId, 
    string AdminRoleInTenant) : IRequest<ServiceResult<IEnumerable<UserInfoDto>>>;