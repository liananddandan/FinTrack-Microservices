using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.Tests.Api.IntegrationTests;

[Collection("IntegrationTests")]
public class TenantMemberChangeRoleTests : IClassFixture<IdentityWebApplicationFactory<Program>>
{
    private readonly IdentityWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TenantMemberChangeRoleTests(IdentityWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ChangeRole_Should_Return_Unauthorized_When_No_Token()
    {
        var response = await _client.PatchAsJsonAsync(
            $"/api/tenant/members/{Guid.NewGuid()}/role",
            new { role = "Admin" });

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, body);
    }

    [Fact]
    public async Task ChangeRole_Should_Return_Ok_When_Promoting_Member_To_Admin()
    {
        var unique = Guid.NewGuid().ToString("N");
        var adminEmail = $"admin-{unique}@test.com";
        const string password = "Password123!";
        var tenantName = $"Tenant-{unique}";

        var seed = await SeedTenantMemberAsync(adminEmail, password, tenantName);

        var tenantToken = await LoginAndSelectTenantAsync(
            adminEmail,
            password,
            seed.TenantPublicId);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tenantToken);

        var response = await _client.PatchAsJsonAsync(
            $"/api/tenant/members/{seed.MemberMembershipPublicId}/role",
            new { role = "Admin" });

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();

        var membership = db.TenantMemberships.First(x => x.PublicId.ToString() == seed.MemberMembershipPublicId);
        membership.Role.Should().Be(TenantRole.Admin);
    }

    [Fact]
    public async Task ChangeRole_Should_Return_BadRequest_When_Changing_Own_Role()
    {
        var unique = Guid.NewGuid().ToString("N");
        var adminEmail = $"admin-{unique}@test.com";
        const string password = "Password123!";
        var tenantName = $"Tenant-{unique}";

        var seed = await SeedAdminOnlyAsync(adminEmail, password, tenantName);

        var tenantToken = await LoginAndSelectTenantAsync(
            adminEmail,
            password,
            seed.TenantPublicId);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tenantToken);

        var response = await _client.PatchAsJsonAsync(
            $"/api/tenant/members/{seed.AdminMembershipPublicId}/role",
            new { role = "Member" });

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
        body.Should().Contain("You cannot change your own role.");
    }

    [Fact]
    public async Task ChangeRole_Should_Return_Ok_When_Demoting_Admin_And_Another_Admin_Still_Exists()
    {
        var unique = Guid.NewGuid().ToString("N");
        var adminEmail = $"admin-{unique}@test.com";
        const string password = "Password123!";
        var tenantName = $"Tenant-{unique}";

        var seed = await SeedAdminOnlyAsync(adminEmail, password, tenantName);

        var tenantToken = await LoginAndSelectTenantAsync(
            adminEmail,
            password,
            seed.TenantPublicId);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tenantToken);

        var response = await _client.PatchAsJsonAsync(
            $"/api/tenant/members/{seed.AnotherAdminMembershipPublicId}/role",
            new { role = "Member" });

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();

        var updatedMembership = db.TenantMemberships
            .First(x => x.PublicId.ToString() == seed.AnotherAdminMembershipPublicId);

        updatedMembership.Role.Should().Be(TenantRole.Member);
    }

    [Fact]
    public async Task ChangeRole_Should_Return_BadRequest_When_Role_Is_Invalid()
    {
        var unique = Guid.NewGuid().ToString("N");
        var adminEmail = $"admin-{unique}@test.com";
        const string password = "Password123!";
        var tenantName = $"Tenant-{unique}";

        var seed = await SeedTenantMemberAsync(adminEmail, password, tenantName);

        var tenantToken = await LoginAndSelectTenantAsync(
            adminEmail,
            password,
            seed.TenantPublicId);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tenantToken);

        var response = await _client.PatchAsJsonAsync(
            $"/api/tenant/members/{seed.MemberMembershipPublicId}/role",
            new { role = "Owner" });

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
        body.Should().Contain("Invalid role.");
    }

    private async Task<string> LoginAndSelectTenantAsync(
        string email,
        string password,
        string tenantPublicId)
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/account/login", new
        {
            email,
            password
        });

        var loginBody = await loginResponse.Content.ReadAsStringAsync();
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK, loginBody);

        var loginResult =
            await loginResponse.Content.ReadFromJsonAsync<ApiResponse<UserLoginResultTestDto>>();

        var accountToken = loginResult!.Data!.Tokens.AccessToken;

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accountToken);

        var selectTenantResponse = await _client.PostAsJsonAsync(
            "/api/account/select-tenant",
            new
            {
                tenantPublicId
            });

        var selectTenantBody = await selectTenantResponse.Content.ReadAsStringAsync();
        selectTenantResponse.StatusCode.Should().Be(HttpStatusCode.OK, selectTenantBody);

        var selectTenantResult =
            await selectTenantResponse.Content.ReadFromJsonAsync<ApiResponse<string>>();

        return selectTenantResult!.Data!;
    }

    private async Task<(string TenantPublicId, string MemberMembershipPublicId)> SeedTenantMemberAsync(
        string adminEmail,
        string password,
        string tenantName)
    {
        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var tenant = new Tenant { Name = tenantName };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            JwtVersion = 1
        };

        await userManager.CreateAsync(admin, password);

        db.TenantMemberships.Add(new TenantMembership
        {
            UserId = admin.Id,
            TenantId = tenant.Id,
            Role = TenantRole.Admin,
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        });

        var member = new ApplicationUser
        {
            UserName = $"member-{Guid.NewGuid():N}@test.com",
            Email = $"member-{Guid.NewGuid():N}@test.com",
            EmailConfirmed = true,
            JwtVersion = 1
        };

        await userManager.CreateAsync(member, password);

        var memberMembership = new TenantMembership
        {
            UserId = member.Id,
            TenantId = tenant.Id,
            Role = TenantRole.Member,
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        };

        db.TenantMemberships.Add(memberMembership);
        await db.SaveChangesAsync();

        return (tenant.PublicId.ToString(), memberMembership.PublicId.ToString());
    }

    private async Task<(string TenantPublicId, string AdminMembershipPublicId, string AnotherAdminMembershipPublicId)> SeedAdminOnlyAsync(
        string adminEmail,
        string password,
        string tenantName)
    {
        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var tenant = new Tenant { Name = tenantName };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            JwtVersion = 1
        };

        await userManager.CreateAsync(admin, password);

        var adminMembership = new TenantMembership
        {
            UserId = admin.Id,
            TenantId = tenant.Id,
            Role = TenantRole.Admin,
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        };

        db.TenantMemberships.Add(adminMembership);

        var anotherAdmin = new ApplicationUser
        {
            UserName = $"admin2-{Guid.NewGuid():N}@test.com",
            Email = $"admin2-{Guid.NewGuid():N}@test.com",
            EmailConfirmed = true,
            JwtVersion = 1
        };

        await userManager.CreateAsync(anotherAdmin, password);

        var anotherAdminMembership = new TenantMembership
        {
            UserId = anotherAdmin.Id,
            TenantId = tenant.Id,
            Role = TenantRole.Admin,
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        };

        db.TenantMemberships.Add(anotherAdminMembership);
        await db.SaveChangesAsync();

        return (
            tenant.PublicId.ToString(),
            adminMembership.PublicId.ToString(),
            anotherAdminMembership.PublicId.ToString());
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
    }

    private sealed class JwtTokenPairTestDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}