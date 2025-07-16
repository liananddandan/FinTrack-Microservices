using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using AutoFixture.Xunit2;
using DotNetCore.CAP;
using FluentAssertions;
using IdentityService.Commands;
using IdentityService.Common.DTOs;
using IdentityService.Common.Results;
using IdentityService.Common.Status;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Services.Interfaces;
using IdentityService.Tests.Attributes;
using Microsoft.EntityFrameworkCore;
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
            Email = "testUserForLoginTest@test.com",
            Password = "TestUserForLogin0@"
        };

        // Act
        var userLoginResponse = await client.PostAsJsonAsync("api/account/login", userLoginCommand);

        // Assert
        var content = await userLoginResponse.Content.ReadAsStringAsync();
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
            Email = "testUserForFirstLoginTest@test.com",
            Password = "TestUserForFirstLogin0@"
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

    [Fact]
    public async Task UserSetPasswordAsync_ShouldReturnSuccess_WhenUserAllGood()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var jwtTokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
        var userDomainService = scope.ServiceProvider.GetRequiredService<IUserDomainService>();
        var user = await userDomainService.GetUserByEmailInnerAsync("testUserForSetPasswordTest@test.com");
        user.Should().NotBeNull();
        var jwtClaimSource = new JwtClaimSource()
        {
            UserPublicId = user.PublicId.ToString(),
            JwtVersion = user.JwtVersion.ToString(),
            TenantPublicId = Guid.NewGuid().ToString(),
            UserRoleInTenant = "ChangePasswordTestRole"
        };
        var tokenResult = await jwtTokenService.GenerateJwtTokenAsync(jwtClaimSource, JwtTokenType.FirstLoginToken);
        tokenResult.Success.Should().BeTrue();
        tokenResult.Data.Should().NotBeNull();
        var userOldPassword = "TestUserForSetPassword0@";
        var userNewPassword = "TestUserForSetPassword1@";
        var reqeust = new ChangePasswordRequest
        {
            OldPassword = userOldPassword,
            NewPassword = userNewPassword
        };
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Data);

        // Act
        var result = await client.PostAsJsonAsync("api/account/set-password", reqeust);
        
        // Assert
        result.EnsureSuccessStatusCode();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();
        var userAfter = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == "testUserForSetPasswordTest@test.com");
        userAfter.Should().NotBeNull();
        userAfter.IsFirstLogin.Should().BeFalse();
        userAfter.JwtVersion.Should().BeGreaterThan(user.JwtVersion);
    }

        [Fact]
    public async Task ResetPasswordAsync_ShouldReturnSuccess_WhenUserAllGood()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var jwtTokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
        var userDomainService = scope.ServiceProvider.GetRequiredService<IUserDomainService>();
        var user = await userDomainService.GetUserByEmailInnerAsync("testUserForResetPasswordTest@test.com");
        user.Should().NotBeNull();
        var jwtClaimSource = new JwtClaimSource()
        {
            UserPublicId = user.PublicId.ToString(),
            JwtVersion = user.JwtVersion.ToString(),
            TenantPublicId = Guid.NewGuid().ToString(),
            UserRoleInTenant = "ChangePasswordTestRole"
        };
        var tokenResult = await jwtTokenService.GenerateJwtTokenAsync(jwtClaimSource, JwtTokenType.FirstLoginToken);
        tokenResult.Success.Should().BeTrue();
        tokenResult.Data.Should().NotBeNull();
        var userOldPassword = "TestUserForResetPassword0@";
        var userNewPassword = "TestUserForResetPassword1@";
        var reqeust = new ChangePasswordRequest
        {
            OldPassword = userOldPassword,
            NewPassword = userNewPassword
        };
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Data);

        // Act
        var result = await client.PostAsJsonAsync("api/account/set-password", reqeust);
        
        // Assert
        result.EnsureSuccessStatusCode();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();
        var userAfter = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == "testUserForResetPasswordTest@test.com");
        userAfter.Should().NotBeNull();
        userAfter.IsFirstLogin.Should().BeFalse();
        userAfter.JwtVersion.Should().BeGreaterThan(user.JwtVersion);
    }
    
    [Fact]
    public async Task RefreshUserJwtTokenAsync_ShouldReturnSuccess_WhenRefreshTokenIsCorrect()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var jwtTokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
        var userDomainService = scope.ServiceProvider.GetRequiredService<IUserDomainService>();
        var user = await userDomainService.GetUserByEmailInnerAsync("testUserForRefreshJwtTokenTest@test.com");
        user.Should().NotBeNull();
        var jwtClaimSource = new JwtClaimSource()
        {
            UserPublicId = user.PublicId.ToString(),
            JwtVersion = user.JwtVersion.ToString(),
            TenantPublicId = Guid.NewGuid().ToString(),
            UserRoleInTenant = "RefreshTokenTestRole"
        };
        var generateResult = await jwtTokenService.GenerateJwtTokenAsync(jwtClaimSource, JwtTokenType.RefreshToken);
        generateResult.Success.Should().BeTrue();
        generateResult.Data.Should().NotBeNull();
        var refreshToken = generateResult.Data;
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", refreshToken);
        
        // Act
        var response = await client.GetAsync("api/account/refresh-token");
        
        // Assert
        string content = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();
        content.Should().NotBeNull();
        content.Should().Contain("100003993");
        content.Should().Contain("accessToken");
        content.Should().Contain("refreshToken");
    }
    
    [Fact]
    public async Task GetUserInfoAsync_ShouldReturnSuccess_WhenAccessTokenIsCorrect()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var jwtTokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
        var userDomainService = scope.ServiceProvider.GetRequiredService<IUserDomainService>();
        var user = await userDomainService.GetUserByEmailInnerAsync("testUserForGetUserInfoTest@test.com");
        user.Should().NotBeNull();
        var jwtClaimSource = new JwtClaimSource()
        {
            UserPublicId = user.PublicId.ToString(),
            JwtVersion = user.JwtVersion.ToString(),
            TenantPublicId = Guid.NewGuid().ToString(),
            UserRoleInTenant = "GetUserTestRole"
        };
        var generateResult = await jwtTokenService.GenerateJwtTokenAsync(jwtClaimSource, JwtTokenType.AccessToken);
        generateResult.Success.Should().BeTrue();
        generateResult.Data.Should().NotBeNull();
        var accessToken = generateResult.Data;
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        // Act
        var response = await client.GetAsync("api/account");
        
        // Assert
        string content = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();
        content.Should().NotBeNull();
        content.Should().Contain("100003992");
        content.Should().Contain("tenantInfoDto");
        content.Should().Contain("email");
    }
    
}