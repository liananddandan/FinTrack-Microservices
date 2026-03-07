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

namespace IdentityService.Tests.IntegrationTests;

[Collection("IntegrationTests")]
public class TenantInvitationResendTests : IClassFixture<IdentityWebApplicationFactory<Program>>
{
    private readonly IdentityWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TenantInvitationResendTests(IdentityWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ResendInvitation_Should_Return_Unauthorized_When_No_Token()
    {
        var invitationPublicId = Guid.NewGuid().ToString();

        var response = await _client.PostAsync(
            $"/api/tenant/invitations/{invitationPublicId}/resend",
            null);

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, body);
    }

    [Fact]
    public async Task ResendInvitation_Should_Return_Ok_When_Request_Is_Valid()
    {
        var unique = Guid.NewGuid().ToString("N");
        var adminEmail = $"admin-{unique}@test.com";
        const string password = "Password123!";
        var tenantName = $"Tenant-{unique}";

        var invitation = await SeedPendingInvitationAsync(adminEmail, password, tenantName);

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

        var response = await _client.PostAsync(
            $"/api/tenant/invitations/{invitation.PublicId}/resend",
            null);

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().BeTrue();
    }

    [Fact]
    public async Task ResendInvitation_Should_Return_BadRequest_When_Invitation_Is_Accepted()
    {
        var unique = Guid.NewGuid().ToString("N");
        var adminEmail = $"admin-{unique}@test.com";
        const string password = "Password123!";
        var tenantName = $"Tenant-{unique}";

        var invitation = await SeedAcceptedInvitationAsync(adminEmail, password, tenantName);

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

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult.Data!.Tokens.AccessToken);

        var response = await _client.PostAsync(
            $"/api/tenant/invitations/{invitation.PublicId}/resend",
            null);

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
        body.Should().Contain("Only pending invitations can be resent.");
    }

    private async Task<TenantInvitation> SeedPendingInvitationAsync(
        string adminEmail,
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

        var invitation = new TenantInvitation
        {
            Email = $"pending-{Guid.NewGuid():N}@test.com",
            TenantId = tenant.Id,
            Tenant = tenant,
            Role = TenantRole.Member,
            Status = InvitationStatus.Pending,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpiredAt = DateTime.UtcNow.AddDays(6),
            Version = 1,
            CreatedByUserId = admin.Id,
            CreatedByUser = admin
        };

        db.TenantInvitations.Add(invitation);
        await db.SaveChangesAsync();

        return invitation;
    }

    private async Task<TenantInvitation> SeedAcceptedInvitationAsync(
        string adminEmail,
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

        var invitation = new TenantInvitation
        {
            Email = $"accepted-{Guid.NewGuid():N}@test.com",
            TenantId = tenant.Id,
            Tenant = tenant,
            Role = TenantRole.Member,
            Status = InvitationStatus.Accepted,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            AcceptedAt = DateTime.UtcNow.AddDays(-1),
            ExpiredAt = DateTime.UtcNow.AddDays(5),
            Version = 2,
            CreatedByUserId = admin.Id,
            CreatedByUser = admin
        };

        db.TenantInvitations.Add(invitation);
        await db.SaveChangesAsync();

        return invitation;
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