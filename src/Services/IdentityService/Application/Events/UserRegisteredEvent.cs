using MediatR;

namespace IdentityService.Application.Events;

public record UserRegisteredEvent(
    long TenantId, 
    Guid TenantPublicId,
    long AdminUserId, 
    string AdminUserName,
    string AdminEmail,
    string TempPassword) : INotification;