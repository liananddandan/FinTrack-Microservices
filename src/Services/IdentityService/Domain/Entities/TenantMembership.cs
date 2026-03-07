using IdentityService.Domain.Enums;

namespace IdentityService.Domain.Entities;

public class TenantMembership
{
    public long Id { get; set; }
    public Guid PublicId { get; set; } = Guid.NewGuid();

    public long UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public long TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public TenantRole Role { get; set; } = TenantRole.Member;

    public bool IsActive { get; set; } = true;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }
    
}