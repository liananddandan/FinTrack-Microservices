using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NotificationService.Application.Abstractions;
using NotificationService.Application.Options;
using SharedKernel.Events;

namespace NotificationService.Application.Services;

public class SmtpEmailService(
    IOptions<SmtpOptions> smtpOptions,
    IOptions<EmailOptions> emailOptions,
    ILogger<SmtpEmailService> logger) : IEmailService
{
    private readonly SmtpOptions _smtpOptions = smtpOptions.Value;
    private readonly EmailOptions _emailOptions = emailOptions.Value;

    public async Task SendEmailAsync(EmailSendRequestedEvent emailEvent)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(_emailOptions.FromName, _emailOptions.FromEmail));
        message.To.Add(new MailboxAddress(emailEvent.ToName ?? emailEvent.To, emailEvent.To));
        message.Subject = emailEvent.Subject;

        message.Body = emailEvent.IsHtml
            ? new TextPart("html") { Text = emailEvent.Body }
            : new TextPart("plain") { Text = emailEvent.Body };

        using var client = new SmtpClient();

        try
        {
            var socketOptions = _smtpOptions.UseSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

            await client.ConnectAsync(_smtpOptions.Host, _smtpOptions.Port, socketOptions);

            if (!string.IsNullOrWhiteSpace(_smtpOptions.User))
            {
                await client.AuthenticateAsync(_smtpOptions.User, _smtpOptions.Password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email via SMTP to {To}", emailEvent.To);
            throw;
        }
    }
}