using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using NotificationService.Options;
using SharedKernel.Events;

namespace NotificationService.Services;

public class SmtpEmailService(
    IOptions<SmtpOptions> smtpOptions,
    ILogger<SmtpEmailService> logger) : IEmailService
{
    private readonly SmtpOptions _smtpOptions = smtpOptions.Value;

    public async Task SendEmailAsync(EmailSendRequestedEvent emailEvent)
    {
        logger.LogInformation($"Received email event: To={emailEvent.To}, Subject={emailEvent.Subject})");
        
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("FinTrack", "no-reply@fintrack.com"));
        var toName = string.IsNullOrWhiteSpace(emailEvent.ToName) ? emailEvent.To : emailEvent.ToName;
        message.To.Add(new MailboxAddress(toName, emailEvent.To));
        message.Subject = emailEvent.Subject;
        message.Body = emailEvent.IsHtml
            ? new TextPart("html") { Text = emailEvent.Body }
            : new TextPart("plain") { Text = emailEvent.Body };
        using var client = new SmtpClient();
        await client.ConnectAsync(_smtpOptions.Host, _smtpOptions.Port, MailKit.Security.SecureSocketOptions.None);
        await client.AuthenticateAsync(_smtpOptions.User, _smtpOptions.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
        
        logger.LogInformation("Email send completed");
    }
}