using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Commands;

public record InviteUserCommand(List<string> Emails,
    string AdminUserPublicId,
    string TenantPublicId,
    string AdminRoleInTenant) : IRequest<ServiceResult<bool>>;