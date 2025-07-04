using System.Net.Http.Json;
using FluentAssertions;
using IdentityService.Commands;
using IdentityService.Common.Results;

namespace IdentityService.Tests.Controllers;

public class TenantControllerTests(IdentityWebApplicationFactory<Program> factory) : IClassFixture<IdentityWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task RegisterTenant_ShouldReturnSuccess()
    {
        // Arrange
        var command = new RegisterTenantCommand(
            TenantName: "Test Tenant Name",
            AdminEmail: "admin-test@email.com",
            AdminName: "TestAdminName"
        );
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/tenant/register", command);
        
        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain(ResultCodes.Tenant.RegisterTenantSuccess);
        content.Should().Contain(command.AdminEmail);
    }
}