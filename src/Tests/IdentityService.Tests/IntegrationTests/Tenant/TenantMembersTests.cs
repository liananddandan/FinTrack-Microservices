using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.Tests.IntegrationTests.Tenant;

[Collection("IntegrationTests")]
public class TenantMembersTests : IClassFixture<IdentityWebApplicationFactory<Program>>
{
    private readonly IdentityWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TenantMembersTests(IdentityWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTenantMembers_Should_Return_Unauthorized_When_No_Token()
    {
        var response = await _client.GetAsync("/api/tenant/members");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, body);
    }

    [Fact]
    public async Task GetTenantMembers_Should_Return_Member_List_When_Request_Is_Valid()
    {
        var unique = Guid.NewGuid().ToString("N");
        var adminEmail = $"admin-{unique}@test.com";
        const string password = "Password123!";
        var tenantName = $"Tenant-{unique}";
        var host = $"tenant-{unique}.test.local";

        var tenantPublicId = await SeedTenantWithTwoMembersAsync(adminEmail, password, tenantName);
        await SeedTenantDomainProjectionAsync(tenantPublicId, host);

        var tenantToken = await LoginAndSelectTenantAsync(adminEmail, password, host);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tenantToken);

        var response = await _client.GetAsync("/api/tenant/members");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<TenantMemberDtoTest>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().NotBeNull();
        apiResponse.Data.Should().HaveCount(2);

        apiResponse.Data.Should().Contain(x => x.Email == adminEmail && x.Role == "Admin");
        apiResponse.Data.Should().Contain(x => x.Role == "Member");
    }

    [Fact]
    public async Task GetTenantMembers_Should_Return_Empty_List_When_Tenant_Has_No_Members()
    {
        var unique = Guid.NewGuid().ToString("N");
        var adminEmail = $"admin-{unique}@test.com";
        const string password = "Password123!";
        var tenantName = $"Tenant-{unique}";
        var host = $"tenant-{unique}.test.local";

        var tenantPublicId = await SeedTenantWithAdminOnlyAsync(adminEmail, password, tenantName);
        await SeedTenantDomainProjectionAsync(tenantPublicId, host);

        var tenantToken = await LoginAndSelectTenantAsync(adminEmail, password, host);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tenantToken);

        var response = await _client.GetAsync("/api/tenant/members");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<TenantMemberDtoTest>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().NotBeNull();
        apiResponse.Data.Should().HaveCount(1);

        apiResponse.Data.Should().ContainSingle(x => x.Email == adminEmail && x.Role == "Admin");
    }

    private async Task<string> LoginAndSelectTenantAsync(
        string email,
        string password,
        string host)
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/account/login", new
        {
            email,
            password
        });

        var loginBody = await loginResponse.Content.ReadAsStringAsync();
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK, loginBody);

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<UserLoginResultTestDto>>();
        loginResult.Should().NotBeNull();
        loginResult!.Data.Should().NotBeNull();
        loginResult.Data!.Tokens.Should().NotBeNull();
        loginResult.Data.Tokens.AccessToken.Should().NotBeNullOrWhiteSpace();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult.Data.Tokens.AccessToken);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/account/select-tenant")
        {
            Content = JsonContent.Create(new { })
        };
        request.Headers.Host = host;

        var selectTenantResponse = await _client.SendAsync(request);
        var selectTenantBody = await selectTenantResponse.Content.ReadAsStringAsync();

        selectTenantResponse.StatusCode.Should().Be(HttpStatusCode.OK, selectTenantBody);

        var selectTenantResult = await selectTenantResponse.Content.ReadFromJsonAsync<ApiResponse<string>>();
        selectTenantResult.Should().NotBeNull();
        selectTenantResult!.Data.Should().NotBeNullOrWhiteSpace();

        return selectTenantResult.Data!;
    }

    private async Task<string> SeedTenantWithTwoMembersAsync(
        string adminEmail,
        string password,
        string tenantName)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var tenant = new Domain.Entities.Tenant
        {
            Name = tenantName
        };

        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            JwtVersion = 1
        };

        var memberEmail = $"member-{Guid.NewGuid():N}@test.com";
        var member = new ApplicationUser
        {
            UserName = memberEmail,
            Email = memberEmail,
            EmailConfirmed = true,
            JwtVersion = 1
        };

        var createAdminResult = await userManager.CreateAsync(admin, password);
        createAdminResult.Succeeded.Should().BeTrue(
            string.Join(", ", createAdminResult.Errors.Select(x => x.Description)));

        var createMemberResult = await userManager.CreateAsync(member, password);
        createMemberResult.Succeeded.Should().BeTrue(
            string.Join(", ", createMemberResult.Errors.Select(x => x.Description)));

        db.TenantMemberships.AddRange(
            new TenantMembership
            {
                UserId = admin.Id,
                TenantId = tenant.Id,
                Role = TenantRole.Admin,
                IsActive = true,
                JoinedAt = DateTime.UtcNow
            },
            new TenantMembership
            {
                UserId = member.Id,
                TenantId = tenant.Id,
                Role = TenantRole.Member,
                IsActive = true,
                JoinedAt = DateTime.UtcNow
            });

        await db.SaveChangesAsync();

        return tenant.PublicId.ToString();
    }

    private async Task<string> SeedTenantWithAdminOnlyAsync(
        string adminEmail,
        string password,
        string tenantName)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var tenant = new Domain.Entities.Tenant
        {
            Name = tenantName
        };

        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            JwtVersion = 1
        };

        var createAdminResult = await userManager.CreateAsync(admin, password);
        createAdminResult.Succeeded.Should().BeTrue(
            string.Join(", ", createAdminResult.Errors.Select(x => x.Description)));

        db.TenantMemberships.Add(new TenantMembership
        {
            UserId = admin.Id,
            TenantId = tenant.Id,
            Role = TenantRole.Admin,
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        return tenant.PublicId.ToString();
    }

    private async Task SeedTenantDomainProjectionAsync(string tenantPublicId, string host)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();

        db.TenantDomainProjections.Add(new TenantDomainProjection
        {
            DomainPublicId = Guid.NewGuid(),
            TenantPublicId = Guid.Parse(tenantPublicId),
            Host = host,
            DomainType = "Custom",
            IsPrimary = true,
            IsActive = true,
            LastSyncedAtUtc = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
    }

    private sealed class ApiResponse<T>
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    private sealed class UserLoginResultTestDto
    {
        public JwtTokenPairTestDto Tokens { get; set; } = new();
        public List<LoginMembershipTestDto> Memberships { get; set; } = new();
    }

    private sealed class JwtTokenPairTestDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    private sealed class LoginMembershipTestDto
    {
        public string TenantPublicId { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    private sealed class TenantMemberDtoTest
    {
        public string UserPublicId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}