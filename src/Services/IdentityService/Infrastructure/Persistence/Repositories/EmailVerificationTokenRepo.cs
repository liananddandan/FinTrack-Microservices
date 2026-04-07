using IdentityService.Application.Accounts.Abstractions;
using IdentityService.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Repositories;

public class EmailVerificationTokenRepo(ApplicationIdentityDbContext dbContext) : IEmailVerificationTokenRepo
{
    public async Task AddAsync(
        EmailVerificationToken token,
        CancellationToken cancellationToken = default)
    {
        await dbContext.EmailVerificationTokens.AddAsync(token, cancellationToken);
    }

    public async Task<List<EmailVerificationToken>> GetActiveTokensByUserIdAsync(
        long userId,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.EmailVerificationTokens
            .Where(x =>
                x.UserId == userId &&
                x.UsedAt == null &&
                x.RevokedAt == null &&
                x.ExpiresAt > now)
            .ToListAsync(cancellationToken);
    }

    public async Task<EmailVerificationToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.EmailVerificationTokens
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
    }

    public async Task<int> CountCreatedSinceAsync(
        long userId,
        DateTime since,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.EmailVerificationTokens
            .CountAsync(x => x.UserId == userId && x.CreatedAt >= since, cancellationToken);
    }

    public async Task<EmailVerificationToken?> GetLatestByUserIdAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.EmailVerificationTokens
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}