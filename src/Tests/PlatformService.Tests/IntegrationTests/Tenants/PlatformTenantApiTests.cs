using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PlatformService.Application.Tenants.Abstractions;
using PlatformService.Application.Tenants.Dtos;

namespace PlatformService.Tests.IntegrationTests.Tenants;

[Collection("IntegrationTests")]
public class PlatformTenantApiTests : IClassFixture<PlatformWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PlatformTenantApiTests(PlatformWebApplicationFactory<Program> factory)
    {
        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IIdentityTenantDirectoryClient>();
                services.AddScoped<IIdentityTenantDirectoryClient, FakeIdentityTenantDirectoryClient>();
            });
        });

        _client = customizedFactory.CreateClient();
    }

    [Fact]
    public async Task GetAllTenants_Should_Return_Tenant_List()
    {
        var response = await _client.GetAsync("/api/platform/tenants");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var apiResponse =
            await response.Content.ReadFromJsonAsync<ApiResponse<List<TenantSummaryDtoTest>>>();

        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().NotBeNull();
        apiResponse.Data!.Count.Should().Be(2);
        apiResponse.Data.Should().Contain(x => x.TenantName == "Auckland Coffee");
        apiResponse.Data.Should().Contain(x => x.TenantName == "Sushi Bar");
    }

    private sealed class FakeIdentityTenantDirectoryClient : IIdentityTenantDirectoryClient
    {
        public Task<IReadOnlyList<TenantSummaryDto>> GetAllTenantsAsync(
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<TenantSummaryDto> result = new List<TenantSummaryDto>
            {
                new()
                {
                    TenantPublicId = Guid.NewGuid().ToString(),
                    TenantName = "Auckland Coffee",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    TenantPublicId = Guid.NewGuid().ToString(),
                    TenantName = "Sushi Bar",
                    IsActive = false,
                    CreatedAt = DateTime.UtcNow
                }
            };

            return Task.FromResult(result);
        }
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