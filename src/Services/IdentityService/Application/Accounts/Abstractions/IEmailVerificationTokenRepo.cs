using IdentityService.Domain.Entities;

namespace IdentityService.Application.Accounts.Abstractions;

public interface IEmailVerificationTokenRepo
{
    Task AddAsync(
        EmailVerificationToken token,
        CancellationToken cancellationToken = default);

    Task<List<EmailVerificationToken>> GetActiveTokensByUserIdAsync(
        long userId,
        DateTime now,
        CancellationToken cancellationToken = default);

    Task<EmailVerificationToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    Task<int> CountCreatedSinceAsync(
        long userId,
        DateTime since,
        CancellationToken cancellationToken = default);

    Task<EmailVerificationToken?> GetLatestByUserIdAsync(
        long userId,
        CancellationToken cancellationToken = default);
    
    Task<EmailVerificationToken?> GetByTokenHashWithUserAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);
}