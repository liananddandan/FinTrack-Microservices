using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace IdentityService.Tests.IntegrationTests.Account;

[Collection("IntegrationTests")]
public class AccountResendVerificationEmailTests(IdentityWebApplicationFactory<Program> factory)
    : IClassFixture<IdentityWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task ResendVerificationEmail_Should_Return_Unauthorized_When_Token_Is_Missing()
    {
        var response = await _client.PostAsync("/api/account/resend-verification-email", null);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, body);
    }

    [Fact]
    public async Task ResendVerificationEmail_Should_Return_BadRequest_When_Email_Is_Already_Verified()
    {
        var unique = Guid.NewGuid().ToString("N");
        var email = $"verified-{unique}@test.com";
        const string password = "Password123!";

        var seeded = await SeedUserAsync(email, password, emailConfirmed: true);
        await ClearResendThrottleAsync(seeded.UserId, email);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", seeded.AccessToken);

        var response = await _client.PostAsync("/api/account/resend-verification-email", null);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
        body.Should().Contain("Email is already verified.");
    }

    [Fact]
    public async Task ResendVerificationEmail_Should_Return_Ok_When_Email_Is_Not_Verified()
    {
        var unique = Guid.NewGuid().ToString("N");
        var email = $"unverified-{unique}@test.com";
        const string password = "Password123!";

        var seeded = await SeedUserAsync(email, password, emailConfirmed: false);
        await ClearResendThrottleAsync(seeded.UserId, email);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", seeded.AccessToken);

        var response = await _client.PostAsync("/api/account/resend-verification-email", null);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Code.Should().Be("EMAIL_VERIFICATION_RESENT");
        apiResponse.Message.Should().Be("Verification email sent successfully.");
        apiResponse.Data.Should().BeTrue();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = await userManager.FindByIdAsync(seeded.UserId.ToString());
        user.Should().NotBeNull();
        user!.EmailConfirmed.Should().BeFalse();

        var token = db.EmailVerificationTokens
            .Where(x => x.UserId == seeded.UserId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault();

        token.Should().NotBeNull();
        token!.TokenHash.Should().NotBeNullOrWhiteSpace();
        token.UsedAt.Should().BeNull();
        token.RevokedAt.Should().BeNull();
        token.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    private async Task<SeedResult> SeedUserAsync(
        string email,
        string password,
        bool emailConfirmed)
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var jwtTokenService = scope.ServiceProvider.GetRequiredService<IdentityService.Application.Common.Abstractions.IJwtTokenService>();

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = emailConfirmed
        };

        var createResult = await userManager.CreateAsync(user, password);
        createResult.Succeeded.Should().BeTrue(string.Join(", ", createResult.Errors.Select(x => x.Description)));

        var accessToken = jwtTokenService.GenerateAccountAccessToken(user);

        return new SeedResult(
            user.Id,
            user.PublicId.ToString(),
            accessToken);
    }

    private async Task ClearResendThrottleAsync(long userId, string email)
    {
        using var scope = factory.Services.CreateScope();
        var redis = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
        var db = redis.GetDatabase();

        var normalizedEmail = email.Trim().ToLowerInvariant();

        var cooldownKey = $"email-throttle:verification:resend:user:{userId}:cooldown";
        var dailyKey = $"email-throttle:verification:resend:email:{normalizedEmail}:daily";

        await db.KeyDeleteAsync(cooldownKey);
        await db.KeyDeleteAsync(dailyKey);
    }

    private sealed record SeedResult(
        long UserId,
        string UserPublicId,
        string AccessToken);

    private sealed class ApiResponse<T>
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}