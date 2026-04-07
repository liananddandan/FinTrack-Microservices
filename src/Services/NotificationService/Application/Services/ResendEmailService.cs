using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using NotificationService.Application.Abstractions;
using NotificationService.Application.Options;
using SharedKernel.Events;

namespace NotificationService.Application.Services;

public class ResendEmailService(
    HttpClient httpClient,
    IOptions<ResendOptions> resendOptions,
    IOptions<EmailOptions> emailOptions,
    ILogger<ResendEmailService> logger)
    : IEmailService
{
    private readonly ResendOptions _resendOptions = resendOptions.Value;
    private readonly EmailOptions _emailOptions = emailOptions.Value;

    public async Task SendEmailAsync(EmailSendRequestedEvent emailEvent)
    {
        var payload = new
        {
            from = $"{_emailOptions.FromName} <{_emailOptions.FromEmail}>",
            to = new[] { emailEvent.To },
            subject = emailEvent.Subject,
            text = emailEvent.IsHtml ? null : emailEvent.Body,
            html = emailEvent.IsHtml ? emailEvent.Body : null
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "/emails");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _resendOptions.ApiKey);
        request.Content = JsonContent.Create(payload);

        try
        {
            var response = await httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError(
                    "Failed to send email via Resend to {To}. StatusCode: {StatusCode}, Response: {Response}",
                    emailEvent.To,
                    response.StatusCode,
                    content);

                response.EnsureSuccessStatusCode();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email via Resend to {To}", emailEvent.To);
            throw;
        }
    }
}