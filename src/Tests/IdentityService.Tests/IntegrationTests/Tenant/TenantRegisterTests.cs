using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.Tests.IntegrationTests.Tenant;

[Collection("IntegrationTests")]
public class TenantRegisterTests(IdentityWebApplicationFactory<Program> factory)
    : IClassFixture<IdentityWebApplicationFactory<Program>>
{
    private const string TestTurnstileToken = "test-turnstile-token";

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
            adminPassword = "Password123!",
            turnstileToken = TestTurnstileToken
        };

        var response = await _client.PostAsJsonAsync("/api/tenant/register", request);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);
        body.Should().Contain("Tenant created successfully");

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<RegisterTenantResultTestDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().NotBeNull();
        apiResponse.Data!.TenantPublicId.Should().NotBeNullOrWhiteSpace();
        apiResponse.Data.UserPublicId.Should().NotBeNull();
        apiResponse.Data.AdminEmail.Should().Be(adminEmail);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();

        var tenant = await db.Tenants.SingleOrDefaultAsync(x => x.Name == tenantName);
        tenant.Should().NotBeNull();

        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == adminEmail);
        user.Should().NotBeNull();
        user!.EmailConfirmed.Should().BeFalse();

        var membership = await db.TenantMemberships.SingleOrDefaultAsync(x =>
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
            adminPassword = "Password123!",
            turnstileToken = TestTurnstileToken
        };

        var response = await _client.PostAsJsonAsync("/api/tenant/register", request);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
        body.Should().Contain("Tenant name is required.");
    }

    [Fact]
    public async Task RegisterTenant_Should_Return_BadRequest_When_TurnstileToken_Is_Missing()
    {
        var unique = Guid.NewGuid().ToString("N");

        var request = new
        {
            tenantName = $"FinTrack-{unique}",
            adminName = "Emily",
            adminEmail = $"emily-{unique}@test.com",
            adminPassword = "Password123!",
            turnstileToken = ""
        };

        var response = await _client.PostAsJsonAsync("/api/tenant/register", request);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
        body.Should().Contain("Verification challenge is required.");
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
            adminPassword = "Password123!",
            turnstileToken = TestTurnstileToken
        };

        var second = new
        {
            tenantName = $"TenantB-{unique}",
            adminName = "Emily",
            adminEmail = $"emily-{unique}@test.com",
            adminPassword = "Password123!",
            turnstileToken = TestTurnstileToken
        };

        var firstResponse = await _client.PostAsJsonAsync("/api/tenant/register", first);
        var firstBody = await firstResponse.Content.ReadAsStringAsync();
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK, firstBody);

        var secondResponse = await _client.PostAsJsonAsync("/api/tenant/register", second);
        var secondBody = await secondResponse.Content.ReadAsStringAsync();

        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, secondBody);
        secondBody.Should().Contain("Admin email already exists.");
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
            adminPassword = "Password123!",
            turnstileToken = TestTurnstileToken
        };

        var second = new
        {
            tenantName = duplicatedTenantName,
            adminName = "Bob",
            adminEmail = $"bob-{unique}@test.com",
            adminPassword = "Password123!",
            turnstileToken = TestTurnstileToken
        };

        var firstResponse = await _client.PostAsJsonAsync("/api/tenant/register", first);
        var firstBody = await firstResponse.Content.ReadAsStringAsync();
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK, firstBody);

        var secondResponse = await _client.PostAsJsonAsync("/api/tenant/register", second);
        var secondBody = await secondResponse.Content.ReadAsStringAsync();

        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, secondBody);
        secondBody.Should().Contain("Tenant name already exists.");
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
            adminPassword = "123",
            turnstileToken = TestTurnstileToken
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

    [Fact]
    public async Task RegisterTenant_Should_Store_AdminEmail_In_Lowercase()
    {
        var unique = Guid.NewGuid().ToString("N");
        var request = new
        {
            tenantName = $"FinTrack-{unique}",
            adminName = "Emily",
            adminEmail = $"Emily-{unique}@Example.com",
            adminPassword = "Password123!",
            turnstileToken = TestTurnstileToken
        };

        var response = await _client.PostAsJsonAsync("/api/tenant/register", request);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<RegisterTenantResultTestDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().NotBeNull();
        apiResponse.Data!.AdminEmail.Should().Be(request.adminEmail.ToLowerInvariant());

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();

        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == request.adminEmail.ToLowerInvariant());
        user.Should().NotBeNull();
        user!.UserName.Should().Be(request.adminEmail.ToLowerInvariant());
    }

    [Fact]
    public async Task RegisterTenant_Should_Create_EmailVerificationToken_For_Admin()
    {
        var unique = Guid.NewGuid().ToString("N");
        var request = new
        {
            tenantName = $"FinTrack-{unique}",
            adminName = "Emily",
            adminEmail = $"emily-{unique}@test.com",
            adminPassword = "Password123!",
            turnstileToken = TestTurnstileToken
        };

        var response = await _client.PostAsJsonAsync("/api/tenant/register", request);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();

        var user = await db.Users.SingleAsync(x => x.Email == request.adminEmail);

        var token = await db.EmailVerificationTokens
            .Where(x => x.UserId == user.Id)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        token.Should().NotBeNull();
        token!.TokenHash.Should().NotBeNullOrWhiteSpace();
        token.UsedAt.Should().BeNull();
        token.RevokedAt.Should().BeNull();
        token.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    private sealed class ApiResponse<T>
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    private sealed class RegisterTenantResultTestDto
    {
        public string TenantPublicId { get; set; } = string.Empty;
        public string UserPublicId { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
    }
}