using SharedKernel.Events;

namespace NotificationService.Services;

public interface IEmailService
{
    Task SendEmailAsync(EmailSendRequestedEvent emailEvent);
}