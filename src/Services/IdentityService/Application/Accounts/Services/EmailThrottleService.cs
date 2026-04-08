using IdentityService.Application.Accounts.Abstractions;
using SharedKernel.Common.Results;
using StackExchange.Redis;

namespace IdentityService.Application.Accounts.Services;

public class EmailThrottleService(
    IConnectionMultiplexer redis)
    : IEmailThrottleService
{
    private const string RegistrationEmailThrottleMinuteKey = "email-throttle:registration:minute";
    private const string VerificationEmailGlobalDailyKey = "email-throttle:verification:daily:global";

    private const int RegistrationEmailLimitPerMinute = 20;
    private const int VerificationEmailGlobalDailyLimit = 90;

    private const int VerificationResendDailyLimitPerEmail = 5;
    private static readonly TimeSpan VerificationResendCooldown = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan RegistrationMinuteWindow = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan VerificationResendDailyWindow = TimeSpan.FromHours(24);

    public async Task<ServiceResult<bool>> CheckRegistrationEmailSendAllowedAsync(
        CancellationToken cancellationToken = default)
    {
        var db = redis.GetDatabase();

        var minuteCountValue = await db.StringGetAsync(RegistrationEmailThrottleMinuteKey);
        var minuteCount = minuteCountValue.HasValue ? (int)minuteCountValue : 0;

        if (minuteCount >= RegistrationEmailLimitPerMinute)
        {
            return ServiceResult<bool>.Fail(
                "REGISTRATION_EMAIL_THROTTLED",
                "Registration succeeded, but verification email was temporarily delayed due to high traffic.");
        }

        var globalDailyCountValue = await db.StringGetAsync(VerificationEmailGlobalDailyKey);
        var globalDailyCount = globalDailyCountValue.HasValue ? (int)globalDailyCountValue : 0;

        if (globalDailyCount >= VerificationEmailGlobalDailyLimit)
        {
            return ServiceResult<bool>.Fail(
                "VERIFICATION_EMAIL_GLOBAL_DAILY_LIMIT_EXCEEDED",
                "Registration succeeded, but verification email sending has reached today's system limit. Please log in later to resend verification email.");
        }

        return ServiceResult<bool>.Ok(
            true,
            "REGISTRATION_EMAIL_ALLOWED",
            "Registration verification email can be sent.");
    }

    public async Task MarkRegistrationEmailSentAsync(
        CancellationToken cancellationToken = default)
    {
        var db = redis.GetDatabase();

        var minuteCount = await db.StringIncrementAsync(RegistrationEmailThrottleMinuteKey);
        if (minuteCount == 1)
        {
            await db.KeyExpireAsync(
                RegistrationEmailThrottleMinuteKey,
                RegistrationMinuteWindow);
        }

        await IncrementGlobalVerificationDailyCountAsync(db);
    }

    public async Task<ServiceResult<bool>> CheckVerificationResendAllowedAsync(
        long userId,
        string email,
        CancellationToken cancellationToken = default)
    {
        var db = redis.GetDatabase();

        var normalizedEmail = NormalizeEmail(email);
        var cooldownKey = GetVerificationResendCooldownKey(userId);
        var dailyKey = GetVerificationResendDailyKey(normalizedEmail);

        var cooldownExists = await db.KeyExistsAsync(cooldownKey);
        if (cooldownExists)
        {
            return ServiceResult<bool>.Fail(
                "EMAIL_VERIFICATION_RESEND_COOLDOWN",
                "Please wait before requesting another verification email.");
        }

        var currentDailyCountValue = await db.StringGetAsync(dailyKey);
        var currentDailyCount = currentDailyCountValue.HasValue ? (int)currentDailyCountValue : 0;

        if (currentDailyCount >= VerificationResendDailyLimitPerEmail)
        {
            return ServiceResult<bool>.Fail(
                "EMAIL_VERIFICATION_RESEND_DAILY_LIMIT_EXCEEDED",
                "You have reached the daily limit for verification emails.");
        }

        var globalDailyCountValue = await db.StringGetAsync(VerificationEmailGlobalDailyKey);
        var globalDailyCount = globalDailyCountValue.HasValue ? (int)globalDailyCountValue : 0;

        if (globalDailyCount >= VerificationEmailGlobalDailyLimit)
        {
            return ServiceResult<bool>.Fail(
                "VERIFICATION_EMAIL_GLOBAL_DAILY_LIMIT_EXCEEDED",
                "Verification email sending has reached today's system limit. Please try again later.");
        }

        return ServiceResult<bool>.Ok(
            true,
            "EMAIL_VERIFICATION_RESEND_ALLOWED",
            "Verification email can be resent.");
    }

    public async Task MarkVerificationResendSentAsync(
        long userId,
        string email,
        CancellationToken cancellationToken = default)
    {
        var db = redis.GetDatabase();

        var normalizedEmail = NormalizeEmail(email);
        var cooldownKey = GetVerificationResendCooldownKey(userId);
        var dailyKey = GetVerificationResendDailyKey(normalizedEmail);

        await db.StringSetAsync(cooldownKey, "1", VerificationResendCooldown);

        var count = await db.StringIncrementAsync(dailyKey);
        if (count == 1)
        {
            await db.KeyExpireAsync(dailyKey, VerificationResendDailyWindow);
        }

        await IncrementGlobalVerificationDailyCountAsync(db);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static string GetVerificationResendCooldownKey(long userId)
    {
        return $"email-throttle:verification:resend:user:{userId}:cooldown";
    }

    private static string GetVerificationResendDailyKey(string normalizedEmail)
    {
        return $"email-throttle:verification:resend:email:{normalizedEmail}:daily";
    }

    private static async Task IncrementGlobalVerificationDailyCountAsync(IDatabase db)
    {
        var count = await db.StringIncrementAsync(VerificationEmailGlobalDailyKey);
        if (count == 1)
        {
            var now = DateTime.UtcNow;
            var tomorrowUtc = now.Date.AddDays(1);
            var ttl = tomorrowUtc - now;

            await db.KeyExpireAsync(VerificationEmailGlobalDailyKey, ttl);
        }
    }
}