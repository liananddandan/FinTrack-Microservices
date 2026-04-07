using Microsoft.AspNetCore.Identity;

namespace IdentityService.Domain.Entities;

public class ApplicationUser : IdentityUser<long>
{
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public long JwtVersion { get; set; } = 1;

    public ICollection<TenantMembership> Memberships { get; set; } = new List<TenantMembership>();
    public ICollection<TenantInvitation> CreatedInvitations { get; set; } = new List<TenantInvitation>();
    public ICollection<EmailVerificationToken> EmailVerificationTokens { get; set; } = new List<EmailVerificationToken>();
}