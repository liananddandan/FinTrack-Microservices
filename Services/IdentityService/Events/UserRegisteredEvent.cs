using MediatR;

namespace IdentityService.Events;

public record UserRegisteredEvent(
    long TenantId, 
    Guid TenantPublicId,
    long AdminUserId, 
    string AdminUserName,
    string AdminEmail,
    string TempPassword) : INotification;