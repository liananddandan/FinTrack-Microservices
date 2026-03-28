using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.Tests.Api.IntegrationTests;

[Collection("IntegrationTests")]

public class TenantRegisterTests(IdentityWebApplicationFactory<Program> factory)
    : IClassFixture<IdentityWebApplicationFactory<Program>>
{
    private readonly IdentityWebApplicationFactory<Program> _factory = factory;
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task RegisterTenant_Should_Create_Tenant_User_And_Membership()
    {
        var unique = Guid.NewGuid().ToString("N");

        var tenantName = $"FinTrack-{unique}";
        var adminEmail = $"emily-{unique}@test.com";

        var request = new
        {
            tenantName,
            adminName = "Emily",
            adminEmail,
            adminPassword = "Password123!"
        };

        var response = await _client.PostAsJsonAsync("/api/tenant/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Tenant created successfully");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();

        var tenant = db.Tenants.SingleOrDefault(x => x.Name == tenantName);
        tenant.Should().NotBeNull();

        var user = db.Users.SingleOrDefault(x => x.Email == adminEmail);
        user.Should().NotBeNull();

        var membership = db.TenantMemberships.SingleOrDefault(x =>
            x.TenantId == tenant!.Id &&
            x.UserId == user!.Id);

        membership.Should().NotBeNull();
        membership!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task RegisterTenant_Should_Return_BadRequest_When_TenantName_Is_Empty()
    {
        var unique = Guid.NewGuid().ToString("N");

        var request = new
        {
            tenantName = "",
            adminName = "Emily",
            adminEmail = $"emily-{unique}@test.com",
            adminPassword = "Password123!"
        };

        var response = await _client.PostAsJsonAsync("/api/tenant/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Tenant name is required.");
    }

    [Fact]
    public async Task RegisterTenant_Should_Return_BadRequest_When_Email_Already_Exists()
    {
        var unique = Guid.NewGuid().ToString("N");

        var first = new
        {
            tenantName = $"TenantA-{unique}",
            adminName = "Emily",
            adminEmail = $"emily-{unique}@test.com",
            adminPassword = "Password123!"
        };

        var second = new
        {
            tenantName = $"TenantB-{unique}",
            adminName = "Emily",
            adminEmail = $"emily-{unique}@test.com",
            adminPassword = "Password123!"
        };
        
        var firstResponse = await _client.PostAsJsonAsync("/api/tenant/register", first);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondResponse = await _client.PostAsJsonAsync("/api/tenant/register", second);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await secondResponse.Content.ReadAsStringAsync();
        body.Should().Contain("Admin email already exists.");
    }

    [Fact]
    public async Task RegisterTenant_Should_Return_BadRequest_When_TenantName_Already_Exists()
    {
        var unique = Guid.NewGuid().ToString("N");
        var duplicatedTenantName = $"FinTrack-{unique}";

        var first = new
        {
            tenantName = duplicatedTenantName,
            adminName = "Emily",
            adminEmail = $"emily1-{unique}@test.com",
            adminPassword = "Password123!"
        };

        var second = new
        {
            tenantName = duplicatedTenantName,
            adminName = "Bob",
            adminEmail = $"bob-{unique}@test.com",
            adminPassword = "Password123!"
        };

        var firstResponse = await _client.PostAsJsonAsync("/api/tenant/register", first);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondResponse = await _client.PostAsJsonAsync("/api/tenant/register", second);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await secondResponse.Content.ReadAsStringAsync();
        body.Should().Contain("Tenant name already exists.");
    }
    
    [Fact]
    public async Task RegisterTenant_Should_Not_Persist_Tenant_When_UserCreation_Fails()
    {
        var unique = Guid.NewGuid().ToString("N");

        var request = new
        {
            tenantName = $"FinTrack-{unique}",
            adminName = "Emily",
            adminEmail = $"emily-{unique}@test.com",
            adminPassword = "123"
        };

        var response = await _client.PostAsJsonAsync("/api/tenant/register", request);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();

        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Name == request.tenantName);
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == request.adminEmail);

        tenant.Should().BeNull();
        user.Should().BeNull();
    }
}