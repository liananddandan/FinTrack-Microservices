using System.Security.Cryptography;
using System.Text;
using IdentityService.Application.Accounts.Abstractions;
using IdentityService.Application.Accounts.Dtos;
using IdentityService.Application.Common.Abstractions;
using IdentityService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Accounts.Services;

public class EmailVerificationService(
    UserManager<ApplicationUser> userManager,
    IEmailVerificationTokenRepo emailVerificationTokenRepo,
    IUnitOfWork unitOfWork)
    : IEmailVerificationService
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(24);

    public async Task<ServiceResult<CreateEmailVerificationTokenResult>> CreateTokenAsync(
        long userId,
        string? createdByIp = null,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return ServiceResult<CreateEmailVerificationTokenResult>.Fail(
                "EMAIL_VERIFICATION_USER_NOT_FOUND",
                "User not found.");
        }

        if (user.EmailConfirmed)
        {
            return ServiceResult<CreateEmailVerificationTokenResult>.Fail(
                "EMAIL_VERIFICATION_ALREADY_CONFIRMED",
                "Email is already verified.");
        }

        var now = DateTime.UtcNow;

        var activeTokens = await emailVerificationTokenRepo.GetActiveTokensByUserIdAsync(
            userId,
            now,
            cancellationToken);

        foreach (var token in activeTokens)
        {
            token.RevokedAt = now;
        }

        var rawToken = GenerateSecureToken();
        var tokenHash = ComputeSha256(rawToken);
        var expiresAt = now.Add(TokenLifetime);

        var entity = new EmailVerificationToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            CreatedAt = now,
            ExpiresAt = expiresAt,
            CreatedByIp = createdByIp
        };

        await emailVerificationTokenRepo.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return ServiceResult<CreateEmailVerificationTokenResult>.Ok(
            new CreateEmailVerificationTokenResult(rawToken, expiresAt),
            "EMAIL_VERIFICATION_TOKEN_CREATED",
            "Email verification token created successfully.");
    }

    private static string GenerateSecureToken()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return WebEncoders.Base64UrlEncode(bytes);
    }

    private static string ComputeSha256(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}