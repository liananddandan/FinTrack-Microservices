using IdentityService.Common.DTOs;
using IdentityService.Domain.Entities;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Commands;

public record GetUsersInTenantCommand(
    string AdminPublicId,
    string TenantPublicId, 
    string AdminRoleInTenant) : IRequest<ServiceResult<IEnumerable<UserInfoDto>>>;