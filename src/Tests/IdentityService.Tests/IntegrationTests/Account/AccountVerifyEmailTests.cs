using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using IdentityService.Application.Accounts.Dtos;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.Tests.IntegrationTests.Account;

[Collection("IntegrationTests")]
public class AccountVerifyEmailTests(IdentityWebApplicationFactory<Program> factory)
    : IClassFixture<IdentityWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task VerifyEmail_Should_Return_BadRequest_When_Token_Is_Empty()
    {
        var request = new VerifyEmailRequest("");

        var response = await _client.PostAsJsonAsync("/api/account/verify-email", request);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Code.Should().Be("EMAIL_VERIFICATION_TOKEN_REQUIRED");
        apiResponse.Message.Should().Be("Verification token is required.");
    }

    [Fact]
    public async Task VerifyEmail_Should_Return_BadRequest_When_Token_Is_Invalid()
    {
        var request = new VerifyEmailRequest("invalid-token");

        var response = await _client.PostAsJsonAsync("/api/account/verify-email", request);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Code.Should().Be("EMAIL_VERIFICATION_TOKEN_INVALID");
        apiResponse.Message.Should().Be("Verification token is invalid.");
    }

    [Fact]
    public async Task VerifyEmail_Should_Return_BadRequest_When_Token_Is_Expired()
    {
        var unique = Guid.NewGuid().ToString("N");
        var email = $"expired-{unique}@test.com";
        const string password = "Password123!";
        const string rawToken = "expired-token";

        await SeedUserWithVerificationTokenAsync(
            email,
            password,
            rawToken,
            expiresAt: DateTime.UtcNow.AddMinutes(-5));

        var request = new VerifyEmailRequest(rawToken);

        var response = await _client.PostAsJsonAsync("/api/account/verify-email", request);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Code.Should().Be("EMAIL_VERIFICATION_TOKEN_EXPIRED");
        apiResponse.Message.Should().Be("Verification token has expired.");
    }

    [Fact]
    public async Task VerifyEmail_Should_Return_BadRequest_When_Token_Is_Revoked()
    {
        var unique = Guid.NewGuid().ToString("N");
        var email = $"revoked-{unique}@test.com";
        const string password = "Password123!";
        const string rawToken = "revoked-token";

        await SeedUserWithVerificationTokenAsync(
            email,
            password,
            rawToken,
            expiresAt: DateTime.UtcNow.AddHours(24),
            revokedAt: DateTime.UtcNow);

        var request = new VerifyEmailRequest(rawToken);

        var response = await _client.PostAsJsonAsync("/api/account/verify-email", request);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Code.Should().Be("EMAIL_VERIFICATION_TOKEN_REVOKED");
        apiResponse.Message.Should().Be("Verification token has been revoked.");
    }

    [Fact]
    public async Task VerifyEmail_Should_Return_Ok_And_Verify_User_When_Token_Is_Valid()
    {
        var unique = Guid.NewGuid().ToString("N");
        var email = $"valid-{unique}@test.com";
        const string password = "Password123!";
        const string rawToken = "valid-email-token";

        var seeded = await SeedUserWithVerificationTokenAsync(
            email,
            password,
            rawToken,
            expiresAt: DateTime.UtcNow.AddHours(24));

        var request = new VerifyEmailRequest(rawToken);

        var response = await _client.PostAsJsonAsync("/api/account/verify-email", request);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Code.Should().Be("EMAIL_VERIFICATION_SUCCESS");
        apiResponse.Message.Should().Be("Email verified successfully.");
        apiResponse.Data.Should().BeTrue();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = await userManager.FindByIdAsync(seeded.UserId.ToString());
        user.Should().NotBeNull();
        user!.EmailConfirmed.Should().BeTrue();

        var tokenHash = ComputeSha256(rawToken);
        var token = await db.EmailVerificationTokens
            .FirstAsync(x => x.TokenHash == tokenHash);

        token.UsedAt.Should().NotBeNull();
    }

    private async Task<SeedResult> SeedUserWithVerificationTokenAsync(
        string email,
        string password,
        string rawToken,
        DateTime expiresAt,
        DateTime? revokedAt = null,
        DateTime? usedAt = null)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = false
        };

        var createResult = await userManager.CreateAsync(user, password);
        createResult.Succeeded.Should().BeTrue(
            string.Join(", ", createResult.Errors.Select(x => x.Description)));

        var token = new EmailVerificationToken
        {
            UserId = user.Id,
            TokenHash = ComputeSha256(rawToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            RevokedAt = revokedAt,
            UsedAt = usedAt
        };

        db.EmailVerificationTokens.Add(token);
        await db.SaveChangesAsync();

        return new SeedResult(user.Id, user.PublicId.ToString(), user.Email!);
    }

    private static string ComputeSha256(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    private sealed record SeedResult(
        long UserId,
        string UserPublicId,
        string Email);

    private sealed class ApiResponse<T>
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}