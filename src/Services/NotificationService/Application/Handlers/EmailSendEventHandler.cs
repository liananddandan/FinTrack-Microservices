using DotNetCore.CAP;
using NotificationService.Application.Abstractions;
using NotificationService.Application.Services;
using SharedKernel.Events;
using SharedKernel.Topics;

namespace NotificationService.Application.Handlers;

public class EmailSendEventHandler(IEmailService emailService) : ICapSubscribe
{
    [CapSubscribe(CapTopics.EmailSend)]
    public async Task HandleEmailSendAsync(EmailSendRequestedEvent sendRequestedEvent)
    {
        await emailService.SendEmailAsync(sendRequestedEvent);
    }
}