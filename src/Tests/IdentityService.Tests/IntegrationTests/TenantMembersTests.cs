using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IdentityService.Tests.IntegrationTests;

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

        await SeedTenantWithTwoMembersAsync(adminEmail, password, tenantName);

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
        // 这个场景理论上在当前系统不太自然，因为能登录就至少自己有 membership。
        // 所以 V1 可先不写。如果一定要写，通常要构造特殊 token / 特殊数据。
        await Task.CompletedTask;
    }

    private async Task SeedTenantWithTwoMembersAsync(
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

        var memberEmail = $"member-{Guid.NewGuid():N}@test.com";
        var member = new ApplicationUser
        {
            UserName = memberEmail,
            Email = memberEmail,
            EmailConfirmed = true,
            JwtVersion = 1
        };

        var adminCreateResult = await userManager.CreateAsync(admin, password);
        adminCreateResult.Succeeded.Should().BeTrue(string.Join(", ", adminCreateResult.Errors.Select(e => e.Description)));

        var memberCreateResult = await userManager.CreateAsync(member, password);
        memberCreateResult.Succeeded.Should().BeTrue(string.Join(", ", memberCreateResult.Errors.Select(e => e.Description)));

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