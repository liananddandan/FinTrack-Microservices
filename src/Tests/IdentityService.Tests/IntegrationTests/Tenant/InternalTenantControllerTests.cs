using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using IdentityService.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.Tests.IntegrationTests.Tenant;

[Collection("IntegrationTests")]
public class InternalTenantControllerTests : IClassFixture<IdentityWebApplicationFactory<Program>>
{
    private readonly IdentityWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    private const string InternalApiKey = "fintrack-internal-dev-key";

    public InternalTenantControllerTests(IdentityWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllTenants_Should_Return_Unauthorized_When_Key_Is_Missing()
    {
        var response = await _client.GetAsync("/internal/tenants");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, body);
        body.Should().Contain("Missing internal API key");
    }

    [Fact]
    public async Task GetAllTenants_Should_Return_Unauthorized_When_Key_Is_Invalid()
    {
        _client.DefaultRequestHeaders.Remove("X-Internal-Key");
        _client.DefaultRequestHeaders.Add("X-Internal-Key", "wrong-key");

        var response = await _client.GetAsync("/internal/tenants");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, body);
        body.Should().Contain("Invalid internal API key");
    }

    [Fact]
    public async Task GetAllTenants_Should_Return_Ok_When_Key_Is_Valid()
    {
        await SeedTenantsAsync();

        _client.DefaultRequestHeaders.Remove("X-Internal-Key");
        _client.DefaultRequestHeaders.Add("X-Internal-Key", InternalApiKey);

        var response = await _client.GetAsync("/internal/tenants");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var apiResponse =
            await response.Content.ReadFromJsonAsync<ApiResponse<List<TenantSummaryDtoTest>>>();

        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().NotBeNull();
        apiResponse.Data!.Count.Should().BeGreaterThanOrEqualTo(2);
        apiResponse.Data.Should().Contain(x => x.TenantName == "Auckland Coffee");
        apiResponse.Data.Should().Contain(x => x.TenantName == "Sushi Bar");
    }

    private async Task SeedTenantsAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();

        if (!db.Tenants.Any(x => x.Name == "Auckland Coffee"))
        {
            db.Tenants.Add(new IdentityService.Domain.Entities.Tenant
            {
                Name = "Auckland Coffee",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (!db.Tenants.Any(x => x.Name == "Sushi Bar"))
        {
            db.Tenants.Add(new IdentityService.Domain.Entities.Tenant
            {
                Name = "Sushi Bar",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync();
    }

    private sealed class ApiResponse<T>
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    private sealed class TenantSummaryDtoTest
    {
        public string TenantPublicId { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}