using System.Net.Http.Json;
using FluentAssertions;
using IdentityService.Commands;
using Xunit.Abstractions;

namespace IdentityService.Tests.Controllers;

public class TenantControllerTests(IdentityWebApplicationFactory<Program> factory,
    ITestOutputHelper testOutputHelper) : IClassFixture<IdentityWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly ITestOutputHelper _output = testOutputHelper;

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
        string responseBody = await response.Content.ReadAsStringAsync();
        _output.WriteLine("Response: " + responseBody);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        _output.WriteLine(content);
    }
}