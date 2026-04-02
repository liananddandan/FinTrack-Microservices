using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Common.DTOs.Auth;

namespace IdentityService.Tests.IntegrationTests.Account;

[Collection("IntegrationTests")]
public class AccountLoginTests(IdentityWebApplicationFactory<Program> factory)
    : IClassFixture<IdentityWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Login_Should_Return_BadRequest_When_Email_Is_Empty()
    {
        var request = new
        {
            email = "",
            password = "Password123!"
        };

        var response = await _client.PostAsJsonAsync("/api/account/login", request);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
        body.Should().Contain("Email is required.");
    }

    [Fact]
    public async Task Login_Should_Return_BadRequest_When_User_Does_Not_Exist()
    {
        var unique = Guid.NewGuid().ToString("N");

        var request = new
        {
            email = $"notfound-{unique}@test.com",
            password = "Password123!"
        };

        var response = await _client.PostAsJsonAsync("/api/account/login", request);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
        body.Should().Contain("Invalid email or password.");
    }

    [Fact]
    public async Task Login_Should_Return_BadRequest_When_Password_Is_Invalid()
    {
        var unique = Guid.NewGuid().ToString("N");
        var email = $"wrongpwd-{unique}@test.com";
        const string correctPassword = "Password123!";

        await SeedUserWithTenantAsync(email, correctPassword, $"Tenant-{unique}", TenantRole.Admin);

        var request = new
        {
            email,
            password = "WrongPassword123!"
        };

        var response = await _client.PostAsJsonAsync("/api/account/login", request);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
        body.Should().Contain("Invalid email or password.");
    }

    [Fact]
    public async Task Login_Should_Return_Ok_With_Tokens_And_Memberships_When_Login_Succeeds()
    {
        var unique = Guid.NewGuid().ToString("N");
        var email = $"success-{unique}@test.com";
        const string password = "Password123!";
        var tenantName = $"FinTrack-{unique}";

        var seeded = await SeedUserWithTenantAsync(
            email,
            password,
            tenantName,
            TenantRole.Admin);

        var request = new
        {
            email,
            password
        };

        var response = await _client.PostAsJsonAsync("/api/account/login", request);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<UserLoginResultTestDto>>();

        apiResponse.Should().NotBeNull();
        apiResponse!.Code.Should().NotBeNullOrWhiteSpace();
        apiResponse.Message.Should().Be("Login successful.");
        apiResponse.Data.Should().NotBeNull();

        apiResponse.Data!.Tokens.AccessToken.Should().NotBeNullOrWhiteSpace();
        apiResponse.Data.Tokens.RefreshToken.Should().NotBeNullOrWhiteSpace();

        apiResponse.Data.Memberships.Should().HaveCount(1);
        var membership = apiResponse.Data.Memberships.Single();
        membership.TenantName.Should().Be(tenantName);
        membership.Role.Should().Be(TenantRole.Admin.ToString());
        membership.TenantPublicId.Should().Be(seeded.TenantPublicId);
    }

    private async Task<SeedResult> SeedUserWithTenantAsync(
        string email,
        string password,
        string tenantName,
        TenantRole role,
        bool isMembershipActive = true)
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
            EmailConfirmed = true
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

        return new SeedResult(
            user.PublicId.ToString(),
            tenant.PublicId.ToString());
    }

    private sealed record SeedResult(
        string UserPublicId,
        string TenantPublicId);

    private sealed class ApiResponse<T>
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    private sealed class UserLoginResultTestDto
    {
        public JwtTokenPair Tokens { get; set; }
        public List<LoginMembershipTestDto> Memberships { get; set; } = new();
    }

    private sealed class LoginMembershipTestDto
    {
        public string TenantPublicId { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}