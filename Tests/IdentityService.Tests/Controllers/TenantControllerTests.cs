using System.Net.Http.Json;
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

namespace IdentityService.Tests.Controllers;

public class TenantControllerTests(IdentityWebApplicationFactory<Program> factory) : IClassFixture<IdentityWebApplicationFactory<Program>>
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
}