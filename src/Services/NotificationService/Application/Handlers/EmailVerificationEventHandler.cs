using DotNetCore.CAP;
using NotificationService.Application.Abstractions;
using SharedKernel.Events;
using SharedKernel.Topics;

namespace NotificationService.Application.Handlers;

public class EmailVerificationEventHandler(
    ILogger<EmailVerificationEmailRequestedEvent> logger,
    IEmailService emailService) : ICapSubscribe
{
    [CapSubscribe(NotificationTopics.EmailVerificationEmailRequested)]
    public async Task HandleAsync(EmailVerificationEmailRequestedEvent notification)
    {
        logger.LogInformation(
            "Received email verification event for {Email}",
            notification.ToEmail);
        
        var subject = "Verify your email address";

        var body = $"""
                    Hello {notification.UserName},

                    Thank you for registering.

                    Please click the link below to verify your email address:

                    {notification.VerificationLink}

                    If you did not create this account, you can ignore this email.
                    """;

        var emailEvent = new EmailSendRequestedEvent
        {
            To = notification.ToEmail,
            ToName = notification.UserName,
            Subject = subject,
            Body = body,
            IsHtml = false
        };

        await emailService.SendEmailAsync(emailEvent);
    }
}