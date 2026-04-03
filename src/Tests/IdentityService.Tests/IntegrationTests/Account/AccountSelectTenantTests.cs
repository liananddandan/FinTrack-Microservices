using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Common.Constants;
using SharedKernel.Common.DTOs.Auth;

namespace IdentityService.Tests.IntegrationTests.Account;

[Collection("IntegrationTests")]
public class AccountSelectTenantTests(IdentityWebApplicationFactory<Program> factory)
    : IClassFixture<IdentityWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task SelectTenant_Should_Return_Unauthorized_When_No_AccountToken()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/account/select-tenant")
        {
            Content = JsonContent.Create(new { })
        };

        request.Headers.Host = "unknown.test.local";

        var response = await _client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, body);
    }

    [Fact]
    public async Task SelectTenant_Should_Return_Ok_With_TenantAccessToken_When_Request_Is_Valid()
    {
        var unique = Guid.NewGuid().ToString("N");
        var email = $"select-tenant-{unique}@test.com";
        const string password = "Password123!";
        var tenantName = $"Tenant-{unique}";
        var host = $"tenant-{unique}.test.local";

        var seeded = await SeedUserWithTenantAsync(email, password, tenantName, TenantRole.Admin);
        await SeedTenantDomainProjectionAsync(seeded.TenantPublicId, host);

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
        loginResult.Data!.Tokens.AccessToken.Should().NotBeNullOrWhiteSpace();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult.Data.Tokens.AccessToken);

        var response = await PostSelectTenantAsync(host);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().NotBeNullOrWhiteSpace();

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(apiResponse.Data);

        jwt.Claims.Should().Contain(x => x.Type == JwtClaimNames.UserId);
        jwt.Claims.Should().Contain(x => x.Type == JwtClaimNames.JwtVersion);
        jwt.Claims.Should().Contain(x => x.Type == JwtClaimNames.Tenant && x.Value == seeded.TenantPublicId);
        jwt.Claims.Should().Contain(x => x.Type == JwtClaimNames.Role && x.Value == TenantRole.Admin.ToString());
        jwt.Claims.Should().Contain(x => x.Type == JwtClaimNames.TokenType && x.Value == JwtTokenType.TenantAccessToken.ToString());
    }

    [Fact]
    public async Task SelectTenant_Should_Return_BadRequest_When_Tenant_Context_Not_Found()
    {
        var unique = Guid.NewGuid().ToString("N");
        var email = $"select-tenant-miss-{unique}@test.com";
        const string password = "Password123!";
        var tenantName = $"Tenant-{unique}";

        await SeedUserWithTenantAsync(email, password, tenantName, TenantRole.Member);

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

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult.Data!.Tokens.AccessToken);

        var response = await PostSelectTenantAsync("unknown.test.local");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
        body.Should().Contain("Tenant context not found from current host.");
    }

    [Fact]
    public async Task SelectTenant_Should_Return_BadRequest_When_Membership_Not_Found()
    {
        var unique = Guid.NewGuid().ToString("N");
        var email = $"select-tenant-no-membership-{unique}@test.com";
        const string password = "Password123!";

        var userTenantName = $"UserTenant-{unique}";
        var targetTenantName = $"TargetTenant-{unique}";
        var targetHost = $"target-{unique}.test.local";

        await SeedUserWithTenantAsync(email, password, userTenantName, TenantRole.Member);
        var targetTenantPublicId = await SeedTenantOnlyAsync(targetTenantName);
        await SeedTenantDomainProjectionAsync(targetTenantPublicId, targetHost);

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

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult.Data!.Tokens.AccessToken);

        var response = await PostSelectTenantAsync(targetHost);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
        body.Should().Contain("Tenant membership not found.");
    }

    private async Task<HttpResponseMessage> PostSelectTenantAsync(string host)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/account/select-tenant")
        {
            Content = JsonContent.Create(new { })
        };

        request.Headers.Host = host;

        return await _client.SendAsync(request);
    }

    private async Task<(string TenantPublicId, string UserPublicId)> SeedUserWithTenantAsync(
        string email,
        string password,
        string tenantName,
        TenantRole role)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var tenant = new Domain.Entities.Tenant
        {
            Name = tenantName
        };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            JwtVersion = 1
        };

        var createResult = await userManager.CreateAsync(user, password);
        createResult.Succeeded.Should().BeTrue(
            string.Join(", ", createResult.Errors.Select(x => x.Description)));

        db.TenantMemberships.Add(new TenantMembership
        {
            TenantId = tenant.Id,
            Tenant = tenant,
            UserId = user.Id,
            User = user,
            Role = role,
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        return (tenant.PublicId.ToString(), user.PublicId.ToString());
    }

    private async Task<string> SeedTenantOnlyAsync(string tenantName)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();

        var tenant = new Domain.Entities.Tenant
        {
            Name = tenantName
        };

        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        return tenant.PublicId.ToString();
    }

    private async Task SeedTenantDomainProjectionAsync(string tenantPublicId, string host)
    {
        using var scope = factory.Services.CreateScope();
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
        public List<LoginMembershipDtoTest> Memberships { get; set; } = new();
    }

    private sealed class JwtTokenPairTestDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    private sealed class LoginMembershipDtoTest
    {
        public string TenantPublicId { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}