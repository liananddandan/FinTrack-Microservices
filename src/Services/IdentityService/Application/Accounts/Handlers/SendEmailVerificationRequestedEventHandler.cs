using DotNetCore.CAP;
using IdentityService.Application.Accounts.Events;
using IdentityService.Application.Common.Options;
using MediatR;
using Microsoft.Extensions.Options;
using SharedKernel.Events;
using SharedKernel.Topics;

namespace IdentityService.Application.Accounts.Handlers;

public class SendEmailVerificationRequestedEventHandler(
    ICapPublisher capPublisher,
    IOptions<FrontendOptions> frontendOptions,
    ILogger<SendEmailVerificationRequestedEventHandler> logger)
    : INotificationHandler<SendEmailVerificationRequestedEvent>
{
    public async Task Handle(
        SendEmailVerificationRequestedEvent notification,
        CancellationToken cancellationToken)
    {
        var portalBaseUrl = frontendOptions.Value.PortalBaseUrl.TrimEnd('/');

        var verifyUrl =
            $"{portalBaseUrl}/account/verify-email?token={Uri.EscapeDataString(notification.EmailVerificationRawToken)}";

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