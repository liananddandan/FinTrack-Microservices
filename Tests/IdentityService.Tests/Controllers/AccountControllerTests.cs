using System.Net.Http.Json;
using System.Text.RegularExpressions;
using AutoFixture.Xunit2;
using DotNetCore.CAP;
using FluentAssertions;
using IdentityService.Commands;
using IdentityService.Common.Results;
using IdentityService.Tests.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SharedKernel.Events;
using SharedKernel.Topics;
using Xunit.Abstractions;

namespace IdentityService.Tests.Controllers;

public class AccountControllerTests(
    IdentityWebApplicationFactory<Program> factory,
    ITestOutputHelper outputHelper) : IClassFixture<IdentityWebApplicationFactory<Program>>
{
    [Theory, AutoMoqData]
    public async Task ConfirmAccountEmailAsync_ShouldReturnResult(
        [Frozen] Mock<ICapPublisher> capPublisherMock)
    {
        // Arrange
        EmailSendRequestedEvent? capturedEmail = null;
        capPublisherMock.Setup(p
                => p.PublishAsync(CapTopics.EmailSend,
                    It.IsAny<EmailSendRequestedEvent>(),
                    It.IsAny<IDictionary<string, string?>>(),
                    It.IsAny<CancellationToken>()))
            .Callback<string, EmailSendRequestedEvent, IDictionary<string, string?>, CancellationToken>(
                (topic, evt, headers, token) => { capturedEmail = evt; })
            .Returns(Task.CompletedTask);

        var client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ICapPublisher));
                if (descriptor != null) services.Remove(descriptor);
                services.AddSingleton(capPublisherMock.Object);
            });
        }).CreateClient();

        var registerTenantCommand = new RegisterTenantCommand("TestTenantName_ConfirmEmail",
            "TestAdminName_ConfirmEmail", "TestAdmin_ConfirmEmail@Test.com");
        var registerTenantResponse = await client.PostAsJsonAsync("api/tenant/register", registerTenantCommand);
        registerTenantResponse.EnsureSuccessStatusCode();
        capturedEmail.Should().NotBeNull();
        capturedEmail.Body.Should().Contain("confirm-email");

        var match = Regex.Match(capturedEmail.Body, @"<a href=""(?<url>[^""]+)"">Verify Email</a>");
        match.Success.Should().BeTrue();
        var url = match.Groups["url"].Value;
        var uri = new Uri(url);
        var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);

        var token = queryParams["token"];
        var userId = queryParams["userId"];
        token.Should().NotBeNullOrEmpty();
        userId.Should().NotBeNullOrEmpty();
        var encodedToken = System.Web.HttpUtility.UrlEncode(token);
        var encodedUserId = System.Web.HttpUtility.UrlEncode(userId);
        var request = $"api/account/confirm-email?token={encodedToken}&userId={encodedUserId}";

        // Act
        var response1 = await client.GetAsync(request);

        // Assert
        response1.EnsureSuccessStatusCode();
        var content = await response1.Content.ReadAsStringAsync();
        content.Should().Contain(ResultCodes.User.UserEmailVerificationSuccess);
    }

    [Fact]
    public async Task UserLoginAsync_ShouldReturnSuccess_WhenAccountAndPasswordIsCorrect()
    {
        // Arrange

        var client = factory.CreateClient();
        var userLoginCommand = new UserLoginCommand()
        {
            Email = "testUserForLogin@test.com",
            Password = "TestUserForLoginPassword0@"
        };

        // Act
        var userLoginResponse = await client.PostAsJsonAsync("api/account/login", userLoginCommand);

        // Assert
        var content = await userLoginResponse.Content.ReadAsStringAsync();
        outputHelper.WriteLine("Response StatusCode: " + userLoginResponse.StatusCode);
        outputHelper.WriteLine("Response Body: " + content);
        content.Should().Contain(ResultCodes.User.UserLoginSuccess);
        userLoginResponse.EnsureSuccessStatusCode();
    }

    [Theory, AutoMoqData]
    public async Task UserLoginAsync_ShouldReturnSuccess_WhenFirstLogin(
        [Frozen] Mock<ICapPublisher> capPublisherMock)
    {
        // Arrange
        EmailSendRequestedEvent? capturedEmail = null;
        capPublisherMock.Setup(p
                => p.PublishAsync(CapTopics.EmailSend,
                    It.IsAny<EmailSendRequestedEvent>(),
                    It.IsAny<IDictionary<string, string?>>(),
                    It.IsAny<CancellationToken>()))
            .Callback<string, EmailSendRequestedEvent, IDictionary<string, string?>, CancellationToken>(
                (topic, evt, headers, token) => { capturedEmail = evt; })
            .Returns(Task.CompletedTask);

        var client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ICapPublisher));
                if (descriptor != null) services.Remove(descriptor);
                services.AddSingleton(capPublisherMock.Object);
            });
        }).CreateClient();

        var userLoginCommand = new UserLoginCommand()
        {
            Email = "testUserForFirstLogin@test.com",
            Password = "TestUserForFirstLoginPassword0@"
        };

        // Act
        var response1 = await client.PostAsJsonAsync("api/account/login", userLoginCommand);

        // Assert
        response1.EnsureSuccessStatusCode();
        var content = await response1.Content.ReadAsStringAsync();
        content.Should().Contain(ResultCodes.User.UserLoginSuccessButFirstLogin);
        capturedEmail.Should().NotBeNull();
        capturedEmail.Body.Should().Contain("Please click the link below to reset your password");
    }
}