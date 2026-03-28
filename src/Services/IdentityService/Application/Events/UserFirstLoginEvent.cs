using MediatR;

namespace IdentityService.Application.Events;

public class UserFirstLoginEvent : INotification
{
    public required string UserPublicId { get; set; }
    public required string JwtVersion { get; set; }
    public required string TenantPublicId { get; set; }
    public required string UserRoleInTenant { get; set; }
    public required string UserEmail { get; set; }
}