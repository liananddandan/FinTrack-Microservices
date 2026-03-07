using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Common.Status;
using IdentityService.Application.Services.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IdentityService.Tests.Api.IntegrationTests;

[Collection("IntegrationTests")]
public class TenantInvitationResolveAcceptTests : IClassFixture<IdentityWebApplicationFactory<Program>>
{
    private readonly IdentityWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TenantInvitationResolveAcceptTests(IdentityWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ResolveInvitation_Should_Return_Unauthorized_When_No_Invite_Token()
    {
        var response = await _client.GetAsync("/api/tenant/invitations/resolve");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, body);
    }

    [Fact]
    public async Task ResolveInvitation_Should_Return_Invitation_Details_When_Token_Is_Valid()
    {
        var invitation = await SeedPendingInvitationAsync();

        var invitationToken = await GenerateInvitationTokenAsync(invitation.PublicId.ToString(), invitation.Version.ToString());

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Invite", invitationToken);

        var response = await _client.GetAsync("/api/tenant/invitations/resolve");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ResolveTenantInvitationResultTestDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().NotBeNull();

        apiResponse.Data!.InvitationPublicId.Should().Be(invitation.PublicId.ToString());
        apiResponse.Data.Email.Should().Be(invitation.Email);
        apiResponse.Data.TenantName.Should().Be(invitation.Tenant.Name);
        apiResponse.Data.Role.Should().Be(invitation.Role.ToString());
        apiResponse.Data.Status.Should().Be(invitation.Status.ToString());
    }

    [Fact]
    public async Task AcceptInvitation_Should_Return_Ok_And_Create_Membership_When_Token_Is_Valid()
    {
        var invitation = await SeedPendingInvitationAsync();

        var invitationToken = await GenerateInvitationTokenAsync(invitation.PublicId.ToString(), invitation.Version.ToString());

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Invite", invitationToken);

        var response = await _client.PostAsync("/api/tenant/invitations/accept", null);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();

        var membership = db.TenantMemberships.FirstOrDefault(x =>
            x.TenantId == invitation.TenantId &&
            x.User.Email == invitation.Email);

        membership.Should().NotBeNull();
        membership!.Role.Should().Be(invitation.Role);

        var updatedInvitation = db.TenantInvitations.First(x => x.Id == invitation.Id);
        updatedInvitation.Status.Should().Be(InvitationStatus.Accepted);
        updatedInvitation.AcceptedAt.Should().NotBeNull();
    }

    private async Task<TenantInvitation> SeedPendingInvitationAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var tenant = new Tenant
        {
            Name = $"Tenant-{Guid.NewGuid():N}"
        };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        var invitedUser = new ApplicationUser
        {
            UserName = $"user-{Guid.NewGuid():N}@test.com",
            Email = $"user-{Guid.NewGuid():N}@test.com",
            EmailConfirmed = true,
            JwtVersion = 1
        };

        var inviter = new ApplicationUser
        {
            UserName = $"admin-{Guid.NewGuid():N}@test.com",
            Email = $"admin-{Guid.NewGuid():N}@test.com",
            EmailConfirmed = true,
            JwtVersion = 1
        };

        (await userManager.CreateAsync(invitedUser, "Password123!")).Succeeded.Should().BeTrue();
        (await userManager.CreateAsync(inviter, "Password123!")).Succeeded.Should().BeTrue();

        var invitation = new TenantInvitation
        {
            Email = invitedUser.Email!,
            TenantId = tenant.Id,
            Tenant = tenant,
            Role = TenantRole.Member,
            Status = InvitationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ExpiredAt = DateTime.UtcNow.AddDays(7),
            Version = 1,
            CreatedByUserId = inviter.Id,
            CreatedByUser = inviter
        };

        db.TenantInvitations.Add(invitation);
        await db.SaveChangesAsync();

        return invitation;
    }

    private async Task<string> GenerateInvitationTokenAsync(string invitationPublicId, string invitationVersion)
    {
        using var scope = _factory.Services.CreateScope();
        var jwtTokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();

        var result = await jwtTokenService.GenerateInvitationTokenAsync(new InvitationClaimSource
        {
            InvitationPublicId = invitationPublicId,
            InvitationVersion = invitationVersion
        });

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNullOrWhiteSpace();

        return result.Data!;
    }

    private sealed class ApiResponse<T>
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    private sealed class ResolveTenantInvitationResultTestDto
    {
        public string InvitationPublicId { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime ExpiredAt { get; set; }
    }
}