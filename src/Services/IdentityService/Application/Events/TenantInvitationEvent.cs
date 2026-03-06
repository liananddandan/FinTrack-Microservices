using IdentityService.Domain.Entities;
using MediatR;

namespace IdentityService.Application.Events;

public record TenantInvitationEvent
(
    ApplicationUser Admin,
    List<string> Emails
) : INotification;