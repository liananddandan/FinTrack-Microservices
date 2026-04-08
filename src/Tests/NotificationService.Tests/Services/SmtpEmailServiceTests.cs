using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NotificationService.Application.Options;
using NotificationService.Application.Services;
using SharedKernel.Events;

namespace NotificationService.Tests.Services;

public class SmtpEmailServiceTests
{
    [Fact]
    public async Task SendEmailAsync_WithValidInput_DoesNotThrow()
    {
        // Arrange
        var smtpOptions = Options.Create(new SmtpOptions
        {
            Host = "localhost",
            Port = 1025,
            User = "",
            Password = "",
            UseSsl = false
        });

        var emailOptions = Options.Create(new EmailOptions
        {
            FromEmail = "noreply@fintrack.local",
            FromName = "FinTrack"
        });

        var loggerMock = new Mock<ILogger<SmtpEmailService>>();

        var emailService = new SmtpEmailService(
            smtpOptions,
            emailOptions,
            loggerMock.Object);

        var emailEvent = new EmailSendRequestedEvent
        {
            To = "to@test.com",
            ToName = "Receiver",
            Subject = "Test Subject",
            Body = "Test Body",
            IsHtml = false
        };

        // Act
        var act = async () => await emailService.SendEmailAsync(emailEvent);

        // Assert
        await act.Should().NotThrowAsync();
    }
}