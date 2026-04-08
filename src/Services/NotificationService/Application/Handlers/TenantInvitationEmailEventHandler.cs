using DotNetCore.CAP;
using NotificationService.Application.Abstractions;
using SharedKernel.Events;
using SharedKernel.Topics;

namespace NotificationService.Application.Handlers;

public class TenantInvitationEmailEventHandler(IEmailService emailService) : ICapSubscribe
{
    [CapSubscribe(NotificationTopics.TenantInvitationEmailRequested)]
    public async Task HandleAsync(TenantInvitationEmailRequestedEvent notification)
    {
        var subject = $"You have been invited to join {notification.TenantName}";

        var body = $"""
                    Hello,

                    You have been invited to join {notification.TenantName} as {notification.Role}.

                    Please click the link below to accept the invitation:

                    {notification.InvitationLink}

                    If you were not expecting this invitation, you can ignore this email.
                    """;

        var emailEvent = new EmailSendRequestedEvent
        {
            To = notification.ToEmail,
            Subject = subject,
            Body = body,
            ToName = notification.ToEmail,
            IsHtml = false
        };

        await emailService.SendEmailAsync(emailEvent);
    }
}