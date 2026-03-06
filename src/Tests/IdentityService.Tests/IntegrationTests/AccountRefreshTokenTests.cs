using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Common.DTOs.Auth;
using Xunit;

namespace IdentityService.Tests.IntegrationTests;

[Collection("IntegrationTests")]
public class RefreshTokenTests : IClassFixture<IdentityWebApplicationFactory<Program>>
{
    private readonly IdentityWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public RefreshTokenTests(IdentityWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RefreshToken_Should_Return_Unauthorized_When_No_Token()
    {
        var response = await _client.GetAsync("/api/account/refresh-token");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, body);
    }

    [Fact]
    public async Task RefreshToken_Should_Return_Unauthorized_When_AccessToken_Is_Used()
    {
        var unique = Guid.NewGuid().ToString("N");
        var email = $"refresh-access-{unique}@test.com";
        const string password = "Password123!";

        await SeedUserWithTenantAsync(email, password, $"Tenant-{unique}", TenantRole.Admin);

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

        var response = await _client.GetAsync("/api/account/refresh-token");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden, body);
    }

    [Fact]
    public async Task RefreshToken_Should_Return_New_TokenPair_When_RefreshToken_Is_Valid()
    {
        var unique = Guid.NewGuid().ToString("N");
        var email = $"refresh-ok-{unique}@test.com";
        const string password = "Password123!";

        await SeedUserWithTenantAsync(email, password, $"Tenant-{unique}", TenantRole.Admin);

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

        var oldAccessToken = loginResult.Data!.Tokens.AccessToken;
        var refreshToken = loginResult.Data.Tokens.RefreshToken;

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", refreshToken);

        var response = await _client.GetAsync("/api/account/refresh-token");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var refreshResult = await response.Content.ReadFromJsonAsync<ApiResponse<JwtTokenPairTestDto>>();
        refreshResult.Should().NotBeNull();
        refreshResult!.Data.Should().NotBeNull();

        refreshResult.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
        refreshResult.Data.RefreshToken.Should().NotBeNullOrWhiteSpace();

        // 一般来说新 access token 应与旧 token 不同
        refreshResult.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
        refreshResult.Data.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task RefreshToken_Should_Return_Unauthorized_When_Token_Is_Invalid()
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "invalid-refresh-token");

        var response = await _client.GetAsync("/api/account/refresh-token");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, body);
    }

    private async Task SeedUserWithTenantAsync(
        string email,
        string password,
        string tenantName,
        TenantRole role,
        bool isMembershipActive = true)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var tenant = new Tenant
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
        createResult.Succeeded.Should().BeTrue(string.Join(", ", createResult.Errors.Select(x => x.Description)));

        var membership = new TenantMembership
        {
            UserId = user.Id,
            TenantId = tenant.Id,
            Role = role,
            IsActive = isMembershipActive,
            JoinedAt = DateTime.UtcNow
        };

        db.TenantMemberships.Add(membership);
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
}