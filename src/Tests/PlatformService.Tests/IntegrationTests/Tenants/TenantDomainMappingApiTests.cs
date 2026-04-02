using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using PlatformService.Domain.Entities;
using PlatformService.Domain.Enums;
using PlatformService.Infrastructure.Persistence;

namespace PlatformService.Tests.IntegrationTests.Tenants;

[Collection("IntegrationTests")]
public class TenantDomainMappingApiTests 
    : IClassFixture<PlatformWebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly PlatformWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TenantDomainMappingApiTests(PlatformWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetByTenantAsync_Should_Return_Domain_Mappings()
    {
        var tenantPublicId = Guid.NewGuid();
        await SeedTenantDomainMappingAsync(
            tenantPublicId,
            "coffee.chenlis.com",
            TenantDomainType.TenantPortal,
            isPrimary: true,
            isActive: true);

        var response = await _client.GetAsync($"/api/platform/tenant-domains/by-tenant/{tenantPublicId}");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var apiResponse =
            await response.Content.ReadFromJsonAsync<ApiResponse<List<TenantDomainMappingDtoTest>>>();

        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().NotBeNull();
        apiResponse.Data!.Should().HaveCount(1);
        apiResponse.Data[0].Host.Should().Be("coffee.chenlis.com");
        apiResponse.Data[0].DomainType.Should().Be("TenantPortal");
        apiResponse.Data[0].IsPrimary.Should().BeTrue();
        apiResponse.Data[0].IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_Should_Create_Domain_Mapping()
    {
        var tenantPublicId = Guid.NewGuid();

        var response = await _client.PostAsJsonAsync("/api/platform/tenant-domains", new
        {
            tenantPublicId,
            host = "coffee.chenlis.com",
            domainType = "TenantPortal",
            isPrimary = true,
            isActive = true
        });

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var apiResponse =
            await response.Content.ReadFromJsonAsync<ApiResponse<TenantDomainMappingDtoTest>>();

        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().NotBeNull();
        apiResponse.Data!.TenantPublicId.Should().Be(tenantPublicId);
        apiResponse.Data.Host.Should().Be("coffee.chenlis.com");
        apiResponse.Data.DomainType.Should().Be("TenantPortal");
        apiResponse.Data.IsPrimary.Should().BeTrue();
        apiResponse.Data.IsActive.Should().BeTrue();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();

        db.TenantDomainMappings.Should().Contain(x =>
            x.TenantPublicId == tenantPublicId &&
            x.Host == "coffee.chenlis.com" &&
            x.DomainType == TenantDomainType.TenantPortal);
    }

    [Fact]
    public async Task CreateAsync_Should_Return_BadRequest_When_Host_Already_Exists()
    {
        var tenantPublicId = Guid.NewGuid();
        await SeedTenantDomainMappingAsync(
            Guid.NewGuid(),
            "coffee.chenlis.com",
            TenantDomainType.TenantPortal,
            isPrimary: true,
            isActive: true);

        var response = await _client.PostAsJsonAsync("/api/platform/tenant-domains", new
        {
            tenantPublicId,
            host = "coffee.chenlis.com",
            domainType = "TenantPortal",
            isPrimary = false,
            isActive = true
        });

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
        body.Should().Contain("Host already exists.");
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Domain_Mapping()
    {
        var existing = await SeedTenantDomainMappingAsync(
            Guid.NewGuid(),
            "old.chenlis.com",
            TenantDomainType.TenantPortal,
            isPrimary: false,
            isActive: false);

        var response = await _client.PutAsJsonAsync(
            $"/api/platform/tenant-domains/{existing.PublicId}",
            new
            {
                host = "admin.coffee.chenlis.com",
                domainType = "TenantAdmin",
                isPrimary = true,
                isActive = true
            });

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var apiResponse =
            await response.Content.ReadFromJsonAsync<ApiResponse<TenantDomainMappingDtoTest>>();

        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().NotBeNull();
        apiResponse.Data!.Host.Should().Be("admin.coffee.chenlis.com");
        apiResponse.Data.DomainType.Should().Be("TenantAdmin");
        apiResponse.Data.IsPrimary.Should().BeTrue();
        apiResponse.Data.IsActive.Should().BeTrue();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();

        var updated = db.TenantDomainMappings.First(x => x.PublicId == existing.PublicId);
        updated.Host.Should().Be("admin.coffee.chenlis.com");
        updated.DomainType.Should().Be(TenantDomainType.TenantAdmin);
        updated.IsPrimary.Should().BeTrue();
        updated.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task SetActiveAsync_Should_Update_Active_Status()
    {
        var existing = await SeedTenantDomainMappingAsync(
            Guid.NewGuid(),
            "coffee.chenlis.com",
            TenantDomainType.TenantPortal,
            isPrimary: true,
            isActive: true);

        var response = await _client.PatchAsJsonAsync(
            $"/api/platform/tenant-domains/{existing.PublicId}/active",
            new
            {
                isActive = false
            });

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var apiResponse =
            await response.Content.ReadFromJsonAsync<ApiResponse<TenantDomainMappingDtoTest>>();

        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().NotBeNull();
        apiResponse.Data!.IsActive.Should().BeFalse();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();

        var updated = db.TenantDomainMappings.First(x => x.PublicId == existing.PublicId);
        updated.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Domain_Mapping()
    {
        var existing = await SeedTenantDomainMappingAsync(
            Guid.NewGuid(),
            "coffee.chenlis.com",
            TenantDomainType.TenantPortal,
            isPrimary: true,
            isActive: true);

        var response = await _client.DeleteAsync(
            $"/api/platform/tenant-domains/{existing.PublicId}");

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var apiResponse =
            await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();

        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().BeTrue();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();

        db.TenantDomainMappings.Should().NotContain(x => x.PublicId == existing.PublicId);
    }

    private async Task<TenantDomainMapping> SeedTenantDomainMappingAsync(
        Guid tenantPublicId,
        string host,
        TenantDomainType domainType,
        bool isPrimary,
        bool isActive)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();

        var mapping = new TenantDomainMapping
        {
            PublicId = Guid.NewGuid(),
            TenantPublicId = tenantPublicId,
            Host = host,
            DomainType = domainType,
            IsPrimary = isPrimary,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };

        db.TenantDomainMappings.Add(mapping);
        await db.SaveChangesAsync();

        return mapping;
    }

    private sealed class ApiResponse<T>
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    private sealed class TenantDomainMappingDtoTest
    {
        public Guid DomainPublicId { get; set; }
        public Guid TenantPublicId { get; set; }
        public string Host { get; set; } = string.Empty;
        public string DomainType { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}