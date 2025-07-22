using System.Net.Mime;
using IdentityService.Domain.Entities;
using MediatR;

namespace IdentityService.Events;

public record TenantInvitationEvent
(
    ApplicationUser Admin,
    List<string> Emails
) : INotification;