namespace IdentityService.Domain.Entities;

public class EmailVerificationToken
{
    public long Id { get; set; }

    public long UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public string TokenHash { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }

    public DateTime? UsedAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    public string? CreatedByIp { get; set; }
}