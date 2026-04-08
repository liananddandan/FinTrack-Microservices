using Microsoft.Extensions.Options;
using NotificationService.Application.Abstractions;
using NotificationService.Application.Options;
using Resend;
using SharedKernel.Events;

namespace NotificationService.Application.Services;

public class ResendEmailService(
    ResendClient resendClient,
    IOptions<EmailOptions> emailOptions,
    ILogger<ResendEmailService> logger)
    : IEmailService
{
    private readonly EmailOptions _emailOptions = emailOptions.Value;

    public async Task SendEmailAsync(EmailSendRequestedEvent emailEvent)
    {
        try
        {
            var message = new EmailMessage
            {
                From = $"{_emailOptions.FromName} <{_emailOptions.FromEmail}>",
                Subject = emailEvent.Subject,
                TextBody = emailEvent.IsHtml ? null : emailEvent.Body,
                HtmlBody = emailEvent.IsHtml ? emailEvent.Body : null
            };

            message.To.Add(emailEvent.To);

            var response = await resendClient.EmailSendAsync(message);

            logger.LogInformation(
                "Email sent successfully via Resend. To: {To}, EmailId: {EmailId}",
                emailEvent.To,
                response.Content);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email via Resend to {To}", emailEvent.To);
            throw;
        }
    }
}