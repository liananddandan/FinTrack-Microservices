using MediatR;

namespace IdentityService.Events;

public record TenantRegisteredEvent(
    long TenantId, 
    Guid PublicTenantId, 
    long AdminUserId, 
    string AdminEmail,
    string TempPassword) : INotification;