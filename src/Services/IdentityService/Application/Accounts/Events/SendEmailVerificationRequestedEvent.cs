using MediatR;

namespace IdentityService.Application.Accounts.Events;

public sealed record SendEmailVerificationRequestedEvent(
    long UserId,
    string Email,
    string UserName,
    string EmailVerificationRawToken,
    DateTime EmailVerificationExpiresAtUtc) : INotification;