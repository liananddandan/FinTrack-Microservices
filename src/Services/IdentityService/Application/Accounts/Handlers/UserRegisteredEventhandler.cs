using DotNetCore.CAP;
using IdentityService.Application.Accounts.Events;
using IdentityService.Application.Common.Options;
using MediatR;
using Microsoft.Extensions.Options;
using SharedKernel.Events;
using SharedKernel.Topics;

namespace IdentityService.Application.Accounts.Handlers;

public class UserRegisteredEventHandler(
    ICapPublisher capPublisher,
    IOptions<FrontendOptions> frontendOptions,
    ILogger<UserRegisteredEventHandler> logger)
    : INotificationHandler<UserRegisteredEvent>
{
    public async Task Handle(
        UserRegisteredEvent notification,
        CancellationToken cancellationToken)
    {
        var portalBaseUrl = frontendOptions.Value.PortalBaseUrl.TrimEnd('/');

        var verifyUrl =
            $"{portalBaseUrl}/verify-email?token={Uri.EscapeDataString(notification.EmailVerificationRawToken)}";

        var integrationEvent = new EmailVerificationEmailRequestedEvent
        {
            ToEmail = notification.Email,
            UserName = notification.UserName,
            VerificationLink = verifyUrl
        };

        await capPublisher.PublishAsync(
            NotificationTopics.EmailVerificationEmailRequested,
            integrationEvent,
            new Dictionary<string, string?>(),
            cancellationToken);

        logger.LogInformation(
            "Published email verification event for user {UserId}",
            notification.UserId);
    }
}