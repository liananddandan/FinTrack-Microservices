using IdentityService.Domain.Entities;
using MediatR;

namespace IdentityService.Application.Events;

public record TenantInvitationCreatedEvent(
    string InvitationPublicId,
    string TenantName,
    string Email
) : INotification;