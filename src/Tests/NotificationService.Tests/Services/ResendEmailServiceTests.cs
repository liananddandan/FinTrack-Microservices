using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NotificationService.Application.Options;
using NotificationService.Application.Services;
using SharedKernel.Events;

namespace NotificationService.Tests.Services;

public class ResendEmailServiceTests
{
    [Fact]
    public async Task SendEmailAsync_WithValidInput_SendsRequestToResend()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
            {
                capturedRequest = request;

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"id\":\"test-email-id\"}")
                };
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://api.resend.com")
        };

        var resendOptions = Options.Create(new ResendOptions
        {
            ApiKey = "test-resend-api-key",
            BaseUrl = "https://api.resend.com"
        });

        var emailOptions = Options.Create(new EmailOptions
        {
            FromEmail = "noreply@test.com",
            FromName = "FinTrack"
        });

        var loggerMock = new Mock<ILogger<ResendEmailService>>();

        var service = new ResendEmailService(
            httpClient,
            resendOptions,
            emailOptions,
            loggerMock.Object);

        var emailEvent = new EmailSendRequestedEvent
        {
            To = "user@test.com",
            ToName = "Test User",
            Subject = "Verify your email",
            Body = "Please verify your email.",
            IsHtml = false
        };

        // Act
        await service.SendEmailAsync(emailEvent);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Post, capturedRequest!.Method);
        Assert.Equal("https://api.resend.com/emails", capturedRequest.RequestUri!.ToString());

        Assert.NotNull(capturedRequest.Headers.Authorization);
        Assert.Equal("Bearer", capturedRequest.Headers.Authorization!.Scheme);
        Assert.Equal("test-resend-api-key", capturedRequest.Headers.Authorization.Parameter);

        handlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SendEmailAsync_WhenResendReturnsError_ThrowsException()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("{\"message\":\"invalid request\"}")
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://api.resend.com")
        };

        var resendOptions = Options.Create(new ResendOptions
        {
            ApiKey = "test-resend-api-key",
            BaseUrl = "https://api.resend.com"
        });

        var emailOptions = Options.Create(new EmailOptions
        {
            FromEmail = "noreply@test.com",
            FromName = "FinTrack"
        });

        var loggerMock = new Mock<ILogger<ResendEmailService>>();

        var service = new ResendEmailService(
            httpClient,
            resendOptions,
            emailOptions,
            loggerMock.Object);

        var emailEvent = new EmailSendRequestedEvent
        {
            To = "user@test.com",
            ToName = "Test User",
            Subject = "Verify your email",
            Body = "Please verify your email.",
            IsHtml = false
        };

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.SendEmailAsync(emailEvent));
    }
}