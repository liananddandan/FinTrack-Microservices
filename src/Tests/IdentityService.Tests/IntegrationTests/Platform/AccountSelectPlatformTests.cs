using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Common.Constants;
using SharedKernel.Common.DTOs.Auth;

namespace IdentityService.Tests.IntegrationTests.Platform;

[Collection("IntegrationTests")]
public class AccountSelectPlatformTests(IdentityWebApplicationFactory<Program> factory)
    : IClassFixture<IdentityWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task SelectPlatform_Should_Return_Unauthorized_When_No_AccountToken()
    {
        var response = await _client.PostAsync("/api/account/select-platform", null);

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, body);
    }

    [Fact]
    public async Task SelectPlatform_Should_Return_Ok_With_PlatformAccessToken_When_Request_Is_Valid()
    {
        var unique = Guid.NewGuid().ToString("N");
        var email = $"select-platform-{unique}@test.com";
        const string password = "Password123!";

        var seeded = await SeedUserWithPlatformAccessAsync(
            email,
            password,
            platformRole: "SuperAdmin");

        var loginResponse = await _client.PostAsJsonAsync("/api/account/login", new
        {
            email,
            password
        });

        var loginBody = await loginResponse.Content.ReadAsStringAsync();
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK, loginBody);

        var loginResult =
            await loginResponse.Content.ReadFromJsonAsync<ApiResponse<UserLoginResultTestDto>>();

        loginResult.Should().NotBeNull();
        loginResult!.Data.Should().NotBeNull();
        loginResult.Data!.Tokens.AccessToken.Should().NotBeNullOrWhiteSpace();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult.Data.Tokens.AccessToken);

        var response = await _client.PostAsync("/api/account/select-platform", null);

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var apiResponse =
            await response.Content.ReadFromJsonAsync<ApiResponse<PlatformTokenDtoTest>>();

        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().NotBeNull();
        apiResponse.Data!.PlatformAccessToken.Should().NotBeNullOrWhiteSpace();
        apiResponse.Data.PlatformRole.Should().Be("SuperAdmin");

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(apiResponse.Data.PlatformAccessToken);

        jwt.Claims.Should().Contain(x => x.Type == JwtClaimNames.UserId && x.Value == seeded.UserPublicId);
        jwt.Claims.Should().Contain(x => x.Type == JwtClaimNames.JwtVersion && x.Value == "1");
        jwt.Claims.Should().Contain(x => x.Type == JwtClaimNames.TokenType &&
                                         x.Value == JwtTokenType.PlatformAccessToken.ToString());
        jwt.Claims.Should().Contain(x => x.Type == "platform_access" && x.Value == "true");
        jwt.Claims.Should().Contain(x => x.Type == "platform_role" && x.Value == "SuperAdmin");
    }

    [Fact]
    public async Task SelectPlatform_Should_Return_BadRequest_When_PlatformAccess_Not_Found()
    {
        var unique = Guid.NewGuid().ToString("N");
        var email = $"select-platform-miss-{unique}@test.com";
        const string password = "Password123!";

        await SeedUserOnlyAsync(email, password);

        var loginResponse = await _client.PostAsJsonAsync("/api/account/login", new
        {
            email,
            password
        });

        var loginBody = await loginResponse.Content.ReadAsStringAsync();
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK, loginBody);

        var loginResult =
            await loginResponse.Content.ReadFromJsonAsync<ApiResponse<UserLoginResultTestDto>>();

        loginResult.Should().NotBeNull();
        loginResult!.Data.Should().NotBeNull();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult.Data!.Tokens.AccessToken);

        var response = await _client.PostAsync("/api/account/select-platform", null);

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
        body.Should().Contain("User does not have platform access.");
    }

    [Fact]
    public async Task SelectPlatform_Should_Return_BadRequest_When_PlatformAccess_Is_Disabled()
    {
        var unique = Guid.NewGuid().ToString("N");
        var email = $"select-platform-disabled-{unique}@test.com";
        const string password = "Password123!";

        await SeedUserWithPlatformAccessAsync(
            email,
            password,
            platformRole: "SuperAdmin",
            isEnabled: false);

        var loginResponse = await _client.PostAsJsonAsync("/api/account/login", new
        {
            email,
            password
        });

        var loginBody = await loginResponse.Content.ReadAsStringAsync();
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK, loginBody);

        var loginResult =
            await loginResponse.Content.ReadFromJsonAsync<ApiResponse<UserLoginResultTestDto>>();

        loginResult.Should().NotBeNull();
        loginResult!.Data.Should().NotBeNull();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult.Data!.Tokens.AccessToken);

        var response = await _client.PostAsync("/api/account/select-platform", null);

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
        body.Should().Contain("User does not have platform access.");
    }

    private async Task<(string UserPublicId, string PlatformRole)> SeedUserWithPlatformAccessAsync(
        string email,
        string password,
        string platformRole,
        bool isEnabled = true)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

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

        db.PlatformAccesses.Add(new PlatformAccess
        {
            UserPublicId = user.PublicId.ToString(),
            Role = platformRole,
            IsEnabled = isEnabled,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        return (user.PublicId.ToString(), platformRole);
    }

    private async Task<string> SeedUserOnlyAsync(
        string email,
        string password)
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

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

        return user.PublicId.ToString();
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

    private sealed class PlatformTokenDtoTest
    {
        public string PlatformAccessToken { get; set; } = string.Empty;
        public string PlatformRole { get; set; } = string.Empty;
    }
}