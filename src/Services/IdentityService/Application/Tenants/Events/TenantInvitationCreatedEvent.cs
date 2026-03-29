using MediatR;

namespace IdentityService.Application.Tenants.Events;

public record TenantInvitationCreatedEvent(
    string InvitationPublicId,
    string TenantName,
    string Email
) : INotification;