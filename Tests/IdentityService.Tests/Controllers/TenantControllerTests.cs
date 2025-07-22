using System.Net;
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
using IdentityService.Domain.Entities;
using IdentityService.Services.Interfaces;
using IdentityService.Tests.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SharedKernel.Events;
using SharedKernel.Topics;
using Xunit.Abstractions;

namespace IdentityService.Tests.Controllers;

public class TenantControllerTests(IdentityWebApplicationFactory<Program> factory,
    ITestOutputHelper testOutputHelper) : IClassFixture<IdentityWebApplicationFactory<Program>>
{

    [Theory, AutoMoqData]
    public async Task RegisterTenant_ShouldReturnSuccess(
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
                (topic, evt, headers, token) =>
                {
                    capturedEmail = evt;
                })
            .Returns(Task.CompletedTask);
        
        var command = new RegisterTenantCommand(
            TenantName: "TestTenantName_RegisterTenant",
            AdminEmail: "AdminTest_RegisterTenant@email.com",
            AdminName: "TestAdminName_RegisterTenant"
        );
        
        // Act
        var response = await factory.CreateClient().PostAsJsonAsync("/api/tenant/register", command);
        
        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain(ResultCodes.Tenant.RegisterTenantSuccess);
        content.Should().Contain(command.AdminEmail);
    }

    [Fact]
    public async Task InviteUserAsync_ShouldReturnBadRequest_WhenEmailIsInvalid()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var jwtTokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
        var userDomainService = scope.ServiceProvider.GetRequiredService<IUserDomainService>();
        var user = await userDomainService.GetUserByEmailWithTenantInnerAsync("testUserForInviteUserTest@test.com");
        user.Should().NotBeNull();
        var jwtClaimSource = new JwtClaimSource()
        {
            UserPublicId = user.PublicId.ToString(),
            JwtVersion = user.JwtVersion.ToString(),
            TenantPublicId = Guid.NewGuid().ToString(),
            UserRoleInTenant = $"Admin_{user.Tenant!.Name}"
        };
        var generateResult = await jwtTokenService.GenerateJwtTokenAsync(jwtClaimSource, JwtTokenType.AccessToken);
        generateResult.Success.Should().BeTrue();
        generateResult.Data.Should().NotBeNull();
        var accessToken = generateResult.Data;
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var inviteRequest = new InviteUserRequest();
        
        // Act
        var response = await client.PostAsJsonAsync("/api/tenant/invite", inviteRequest);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Theory, AutoMoqData]
    public async Task InviteUserAsync_ShouldReturnSuccess(
        [Frozen] Mock<ICapPublisher> capPublisherMock)
    {
        // Arrange
        var client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ICapPublisher));
                if (descriptor != null) services.Remove(descriptor);
                services.AddSingleton(capPublisherMock.Object);
            });
        }).CreateClient();
        EmailSendRequestedEvent? capturedEmail = null;
        var count = 0;
        capPublisherMock.Setup(p
                => p.PublishAsync(CapTopics.EmailSend,
                    It.IsAny<EmailSendRequestedEvent>(),
                    It.IsAny<IDictionary<string, string?>>(),
                    It.IsAny<CancellationToken>()))
            .Callback<string, EmailSendRequestedEvent, IDictionary<string, string?>, CancellationToken>(
                (topic, evt, headers, token) =>
                {
                    capturedEmail = evt;
                    count++;
                })
            .Returns(Task.CompletedTask);
        using var scope = factory.Services.CreateScope();
        var jwtTokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
        var userDomainService = scope.ServiceProvider.GetRequiredService<IUserDomainService>();
        var user = await userDomainService.GetUserByEmailWithTenantInnerAsync("testUserForInviteUserTest@test.com");
        user.Should().NotBeNull();
        var jwtClaimSource = new JwtClaimSource()
        {
            UserPublicId = user.PublicId.ToString(),
            JwtVersion = user.JwtVersion.ToString(),
            TenantPublicId = user.Tenant!.PublicId.ToString(),
            UserRoleInTenant = $"Admin_{user.Tenant!.Name}"
        };
        var generateResult = await jwtTokenService.GenerateJwtTokenAsync(jwtClaimSource, JwtTokenType.AccessToken);
        generateResult.Success.Should().BeTrue();
        generateResult.Data.Should().NotBeNull();
        var accessToken = generateResult.Data;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var inviteRequest = new InviteUserRequest()
        {
            Emails = new List<string> { "1@gmail.com","2@gmail.com","3@gmail.com" },
        };
        
        // Act
        var response = await client.PostAsJsonAsync("/api/tenant/invite", inviteRequest);
        
        // Assert
        response.EnsureSuccessStatusCode();
        count.Should().Be(inviteRequest.Emails.Count);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain(ResultCodes.Tenant.InvitationUsersStartSuccess);
    }
    
    [Theory, AutoMoqData]
    public async Task ReceiveInviteAsync_ShouldReturnSuccess(
        [Frozen] Mock<ICapPublisher> capPublisherMock)
    {
        // Arrange
        var client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ICapPublisher));
                if (descriptor != null) services.Remove(descriptor);
                services.AddSingleton(capPublisherMock.Object);
            });
        }).CreateClient();
        EmailSendRequestedEvent? capturedEmail = null;
        capPublisherMock.Setup(p
                => p.PublishAsync(CapTopics.EmailSend,
                    It.IsAny<EmailSendRequestedEvent>(),
                    It.IsAny<IDictionary<string, string?>>(),
                    It.IsAny<CancellationToken>()))
            .Callback<string, EmailSendRequestedEvent, IDictionary<string, string?>, CancellationToken>(
                (topic, evt, headers, token) =>
                {
                    capturedEmail = evt;
                })
            .Returns(Task.CompletedTask);
        using var scope = factory.Services.CreateScope();
        var jwtTokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
        var userDomainService = scope.ServiceProvider.GetRequiredService<IUserDomainService>();
        var user = await userDomainService.GetUserByEmailWithTenantInnerAsync("testUserForInviteUserTest@test.com");
        user.Should().NotBeNull();
        var jwtClaimSource = new JwtClaimSource()
        {
            UserPublicId = user.PublicId.ToString(),
            JwtVersion = user.JwtVersion.ToString(),
            TenantPublicId = user.Tenant!.PublicId.ToString(),
            UserRoleInTenant = $"Admin_{user.Tenant!.Name}"
        };
        var generateResult = await jwtTokenService.GenerateJwtTokenAsync(jwtClaimSource, JwtTokenType.AccessToken);
        generateResult.Success.Should().BeTrue();
        generateResult.Data.Should().NotBeNull();
        var accessToken = generateResult.Data;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var inviteRequest = new InviteUserRequest()
        {
            Emails = new List<string> { $"receive_invitation_{Guid.NewGuid().ToString()}@gmail.com"},
        };
        var response = await client.PostAsJsonAsync("/api/tenant/invite", inviteRequest);
        response.EnsureSuccessStatusCode();
        var match = Regex.Match(capturedEmail!.Body, @"Token is ([\w\-\.]+)", RegexOptions.IgnoreCase);
        match.Success.Should().BeTrue();
        string invitationToken = match.Groups[1].Value;
        
        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/tenant/receive-invite");
        request.Headers.Authorization = new AuthenticationHeaderValue("Invite", invitationToken);
        var receiveResponse = await client.SendAsync(request);
        
        // Assert
        receiveResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetUsersInfoInTenantAsync_ShouldReturnSuccess()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var jwtTokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
        var userDomainService = scope.ServiceProvider.GetRequiredService<IUserDomainService>();
        var user = await userDomainService.GetUserByEmailWithTenantInnerAsync("testUserForGetUsersInTenantTest@test.com");
        user.Should().NotBeNull();
        var jwtClaimSource = new JwtClaimSource()
        {
            UserPublicId = user.PublicId.ToString(),
            JwtVersion = user.JwtVersion.ToString(),
            TenantPublicId = user.Tenant.PublicId.ToString(),
            UserRoleInTenant = $"Admin_{user.Tenant!.Name}"
        };
        var generateResult = await jwtTokenService.GenerateJwtTokenAsync(jwtClaimSource, JwtTokenType.AccessToken);
        generateResult.Success.Should().BeTrue();
        generateResult.Data.Should().NotBeNull();
        var accessToken = generateResult.Data;
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Act
        var result = await client.GetAsync("/api/tenant/users");
        
        //Assert
        result.EnsureSuccessStatusCode();
        var content = await result.Content.ReadAsStringAsync();
        testOutputHelper.WriteLine(content);
    }
}