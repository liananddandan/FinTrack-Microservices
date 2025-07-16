using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Commands;

public class InviteUserCommand : IRequest<ServiceResult<bool>>
{
    public List<string> Emails { get; set; } = new ();
    public required string AdminUserPublicId { get; set; }
    public required string AdminJwtVersion { get; set; }
    public required string TenantPublicid { get; set; }
    public required string AdminRoleInTenant { get; set; }
}