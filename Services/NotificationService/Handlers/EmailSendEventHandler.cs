using DotNetCore.CAP;
using NotificationService.Services;
using SharedKernel.Events;
using SharedKernel.Topics;

namespace NotificationService.Handlers;

public class EmailSendEventHandler(IEmailService emailService) : ICapSubscribe
{
    [CapSubscribe(CapTopics.EmailSend)]
    public async Task HandleEmailSendAsync(EmailSendRequestedEvent sendRequestedEvent)
    {
        await emailService.SendEmailAsync(sendRequestedEvent);
    }
}