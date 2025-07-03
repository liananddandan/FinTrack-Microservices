using MediatR;

namespace IdentityService.Events;

public record TenantRegisteredEvent(
    long TenantId, 
    Guid TenantPublicId,
    long AdminUserId, 
    string AdminUserName,
    string AdminEmail,
    string TempPassword) : INotification;