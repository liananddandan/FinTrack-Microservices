using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using IdentityService.Application.Common.Status;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IdentityService.Tests.Api.IntegrationTests;

[Collection("IntegrationTests")]
public class TenantInvitationTests : IClassFixture<IdentityWebApplicationFactory<Program>>
{
    private readonly IdentityWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TenantInvitationTests(IdentityWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateInvitation_Should_Return_Unauthorized_When_No_Token()
    {
        var request = new
        {
            email = "user@test.com",
            role = "Member"
        };

        var response = await _client.PostAsJsonAsync("/api/tenant/invitations", request);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, body);
    }

    [Fact]
    public async Task CreateInvitation_Should_Return_Ok_And_Save_Invitation_When_Request_Is_Valid()
    {
        var unique = Guid.NewGuid().ToString("N");
        var adminEmail = $"admin-{unique}@test.com";
        var memberEmail = $"member-{unique}@test.com";
        const string password = "Password123!";
        var tenantName = $"Tenant-{unique}";

        var tenantPublicId = await SeedAdminAndUserAsync(adminEmail, memberEmail, password, tenantName);

        // login -> account token
        var loginResponse = await _client.PostAsJsonAsync("/api/account/login", new
        {
            email = adminEmail,
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

        // select tenant -> tenant token
        var selectTenantResponse = await _client.PostAsJsonAsync(
            "/api/account/select-tenant",
            new
            {
                tenantPublicId
            });

        var selectTenantBody = await selectTenantResponse.Content.ReadAsStringAsync();
        selectTenantResponse.StatusCode.Should().Be(HttpStatusCode.OK, selectTenantBody);

        var selectTenantResult = await selectTenantResponse.Content.ReadFromJsonAsync<ApiResponse<string>>();
        selectTenantResult.Should().NotBeNull();
        selectTenantResult!.Data.Should().NotBeNullOrWhiteSpace();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", selectTenantResult.Data);

        // create invitation with tenant token
        var request = new
        {
            email = memberEmail,
            role = "Member"
        };

        var response = await _client.PostAsJsonAsync("/api/tenant/invitations", request);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();

        var invitation = db.TenantInvitations.FirstOrDefault(x => x.Email == memberEmail);
        invitation.Should().NotBeNull();
        invitation!.Status.Should().Be(InvitationStatus.Pending);
        invitation.Role.Should().Be(TenantRole.Member);
    }

    private async Task<string> SeedAdminAndUserAsync(
        string adminEmail,
        string memberEmail,
        string password,
        string tenantName)
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

        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            JwtVersion = 1
        };

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