using DotNetCore.CAP;
using EmailService.Options;
using EmailService.Services;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using SharedKernel.Events;
using SharedKernel.Topics;

namespace EmailService.Handlers;

public class EmailSendEventHandler(IEmailService emailService) : ICapSubscribe
{
    [CapSubscribe(CapTopics.EmailSend)]
    public async Task HandleEmailSendAsync(EmailSendRequestedEvent sendRequestedEvent)
    {
        await emailService.SendEmailAsync(sendRequestedEvent);
    }
}