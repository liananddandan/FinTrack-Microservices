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
public class TenantMemberRemoveTests : IClassFixture<IdentityWebApplicationFactory<Program>>
{
    private readonly IdentityWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TenantMemberRemoveTests(IdentityWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RemoveMember_Should_Return_Unauthorized_When_No_Token()
    {
        var membershipPublicId = Guid.NewGuid().ToString();

        var response = await _client.DeleteAsync(
            $"/api/tenant/members/{membershipPublicId}");

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, body);
    }

    [Fact]
    public async Task RemoveMember_Should_Return_Ok_When_Request_Is_Valid()
    {
        var unique = Guid.NewGuid().ToString("N");
        var adminEmail = $"admin-{unique}@test.com";
        const string password = "Password123!";
        var tenantName = $"Tenant-{unique}";

        var seed = await SeedTenantMemberAsync(adminEmail, password, tenantName);

        // login
        var loginResponse = await _client.PostAsJsonAsync("/api/account/login", new
        {
            email = adminEmail,
            password
        });

        var loginBody = await loginResponse.Content.ReadAsStringAsync();
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK, loginBody);

        var loginResult =
            await loginResponse.Content.ReadFromJsonAsync<ApiResponse<UserLoginResultTestDto>>();

        var accountToken = loginResult!.Data!.Tokens.AccessToken;

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accountToken);

        // select tenant
        var selectTenantResponse = await _client.PostAsJsonAsync(
            "/api/account/select-tenant",
            new
            {
                tenantPublicId = seed.TenantPublicId
            });

        var selectTenantBody = await selectTenantResponse.Content.ReadAsStringAsync();

        selectTenantResponse.StatusCode.Should().Be(HttpStatusCode.OK, selectTenantBody);

        var selectTenantResult =
            await selectTenantResponse.Content.ReadFromJsonAsync<ApiResponse<string>>();

        var tenantToken = selectTenantResult!.Data!;

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tenantToken);

        // remove member
        var response = await _client.DeleteAsync(
            $"/api/tenant/members/{seed.MembershipPublicId}");

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var apiResponse =
            await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();

        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveMember_Should_Return_BadRequest_When_Remove_Self()
    {
        var unique = Guid.NewGuid().ToString("N");
        var adminEmail = $"admin-{unique}@test.com";
        const string password = "Password123!";
        var tenantName = $"Tenant-{unique}";

        var seed = await SeedAdminMembershipAsync(adminEmail, password, tenantName);

        // login
        var loginResponse = await _client.PostAsJsonAsync("/api/account/login", new
        {
            email = adminEmail,
            password
        });

        var loginBody = await loginResponse.Content.ReadAsStringAsync();
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK, loginBody);

        var loginResult =
            await loginResponse.Content.ReadFromJsonAsync<ApiResponse<UserLoginResultTestDto>>();

        var accountToken = loginResult!.Data!.Tokens.AccessToken;

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accountToken);

        // select tenant
        var selectTenantResponse = await _client.PostAsJsonAsync(
            "/api/account/select-tenant",
            new
            {
                tenantPublicId = seed.TenantPublicId
            });

        var selectTenantResult =
            await selectTenantResponse.Content.ReadFromJsonAsync<ApiResponse<string>>();

        var tenantToken = selectTenantResult!.Data!;

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tenantToken);

        // try remove self
        var response = await _client.DeleteAsync(
            $"/api/tenant/members/{seed.MembershipPublicId}");

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
        body.Should().Contain("You cannot remove yourself.");
    }
    
    [Fact]
    public async Task RemoveMember_Should_Return_BadRequest_When_Member_Not_Found()
    {
        var unique = Guid.NewGuid().ToString("N");
        var adminEmail = $"admin-{unique}@test.com";
        const string password = "Password123!";
        var tenantName = $"Tenant-{unique}";

        var seed = await SeedAdminMembershipAsync(adminEmail, password, tenantName);

        // login
        var loginResponse = await _client.PostAsJsonAsync("/api/account/login", new
        {
            email = adminEmail,
            password
        });

        var loginResult =
            await loginResponse.Content.ReadFromJsonAsync<ApiResponse<UserLoginResultTestDto>>();

        var accountToken = loginResult!.Data!.Tokens.AccessToken;

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accountToken);

        // select tenant
        var selectTenantResponse = await _client.PostAsJsonAsync(
            "/api/account/select-tenant",
            new
            {
                tenantPublicId = seed.TenantPublicId
            });

        var selectTenantResult =
            await selectTenantResponse.Content.ReadFromJsonAsync<ApiResponse<string>>();

        var tenantToken = selectTenantResult!.Data!;

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tenantToken);

        var randomMembershipId = Guid.NewGuid();

        var response = await _client.DeleteAsync(
            $"/api/tenant/members/{randomMembershipId}");

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
        body.Should().Contain("Membership not found");
    }
    
    [Fact]
    public async Task RemoveMember_Should_Return_BadRequest_When_Member_Already_Removed()
    {
        var unique = Guid.NewGuid().ToString("N");
        var adminEmail = $"admin-{unique}@test.com";
        const string password = "Password123!";
        var tenantName = $"Tenant-{unique}";

        var seed = await SeedRemovedMemberAsync(adminEmail, password, tenantName);

        // login
        var loginResponse = await _client.PostAsJsonAsync("/api/account/login", new
        {
            email = adminEmail,
            password
        });

        var loginResult =
            await loginResponse.Content.ReadFromJsonAsync<ApiResponse<UserLoginResultTestDto>>();

        var accountToken = loginResult!.Data!.Tokens.AccessToken;

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accountToken);

        // select tenant
        var selectTenantResponse = await _client.PostAsJsonAsync(
            "/api/account/select-tenant",
            new
            {
                tenantPublicId = seed.TenantPublicId
            });

        var selectTenantResult =
            await selectTenantResponse.Content.ReadFromJsonAsync<ApiResponse<string>>();

        var tenantToken = selectTenantResult!.Data!;

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tenantToken);

        var response = await _client.DeleteAsync(
            $"/api/tenant/members/{seed.MembershipPublicId}");

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
        body.Should().Contain("Member already removed");
    }
    
    private async Task<(string MembershipPublicId, string TenantPublicId)> SeedRemovedMemberAsync(
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

        var membership = new TenantMembership
        {
            UserId = member.Id,
            TenantId = tenant.Id,
            Role = TenantRole.Member,
            IsActive = false,
            JoinedAt = DateTime.UtcNow,
            LeftAt = DateTime.UtcNow
        };

        db.TenantMemberships.Add(membership);

        await db.SaveChangesAsync();

        return (membership.PublicId.ToString(), tenant.PublicId.ToString());
    }
    
    private async Task<(string MembershipPublicId, string TenantPublicId)> SeedTenantMemberAsync(
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

        var membership = new TenantMembership
        {
            UserId = member.Id,
            TenantId = tenant.Id,
            Role = TenantRole.Member,
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        };

        db.TenantMemberships.Add(membership);

        await db.SaveChangesAsync();

        return (membership.PublicId.ToString(), tenant.PublicId.ToString());
    }

    private async Task<(string MembershipPublicId, string TenantPublicId)> SeedAdminMembershipAsync(
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

        await userManager.CreateAsync(admin, password);

        var membership = new TenantMembership
        {
            UserId = admin.Id,
            TenantId = tenant.Id,
            Role = TenantRole.Admin,
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        };

        db.TenantMemberships.Add(membership);
        await db.SaveChangesAsync();

        return (membership.PublicId.ToString(), tenant.PublicId.ToString());
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