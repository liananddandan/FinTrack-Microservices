using SharedKernel.Events;

namespace EmailService.Services;

public interface IEmailService
{
    Task SendEmailAsync(EmailSendRequestedEvent emailEvent);
}