using SharedKernel.Events;

namespace NotificationService.Application.Abstractions;

public interface IEmailService
{
    Task SendEmailAsync(EmailSendRequestedEvent emailEvent);
}