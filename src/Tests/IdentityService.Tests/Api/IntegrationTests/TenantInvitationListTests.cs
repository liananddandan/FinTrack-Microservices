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
public class TenantInvitationListTests : IClassFixture<IdentityWebApplicationFactory<Program>>
{
    private readonly IdentityWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TenantInvitationListTests(IdentityWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTenantInvitations_Should_Return_Unauthorized_When_No_Token()
    {
        var response = await _client.GetAsync("/api/tenant/invitations");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, body);
    }

    [Fact]
    public async Task GetTenantInvitations_Should_Return_Invitation_List_When_Request_Is_Valid()
    {
        var unique = Guid.NewGuid().ToString("N");
        var adminEmail = $"admin-{unique}@test.com";
        const string password = "Password123!";
        var tenantName = $"Tenant-{unique}";

        await SeedTenantWithAdminAndInvitationsAsync(adminEmail, password, tenantName);

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

        var response = await _client.GetAsync("/api/tenant/invitations");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<TenantInvitationDtoTest>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().NotBeNull();
        apiResponse.Data.Should().HaveCount(2);

        apiResponse.Data.Should().Contain(x =>
            x.Status == "Pending" &&
            x.Role == "Member" &&
            x.CreatedByUserEmail == adminEmail);

        apiResponse.Data.Should().Contain(x =>
            x.Status == "Accepted" &&
            x.Role == "Admin" &&
            x.CreatedByUserEmail == adminEmail);
    }

    private async Task SeedTenantWithAdminAndInvitationsAsync(
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

        db.TenantInvitations.AddRange(
            new TenantInvitation
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
            },
            new TenantInvitation
            {
                Email = $"accepted-{Guid.NewGuid():N}@test.com",
                TenantId = tenant.Id,
                Tenant = tenant,
                Role = TenantRole.Admin,
                Status = InvitationStatus.Accepted,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                AcceptedAt = DateTime.UtcNow.AddDays(-2),
                ExpiredAt = DateTime.UtcNow.AddDays(4),
                Version = 2,
                CreatedByUserId = admin.Id,
                CreatedByUser = admin
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
    }

    private sealed class JwtTokenPairTestDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    private sealed class TenantInvitationDtoTest
    {
        public string InvitationPublicId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime ExpiredAt { get; set; }
        public string CreatedByUserEmail { get; set; } = string.Empty;
    }
}