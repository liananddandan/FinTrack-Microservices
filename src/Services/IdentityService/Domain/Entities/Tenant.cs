namespace IdentityService.Domain.Entities;

public class Tenant
{
    public long Id { get; set; }
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public string? PrimaryDomain { get; set; }
    public string? LogoUrl { get; set; }
    public string? ThemeColor { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? OpeningHours { get; set; }
    public string Currency { get; set; } = "NZD";
    public string TimeZone { get; set; } = "Pacific/Auckland";
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    
    public string? StripeConnectedAccountId { get; set; }
    public bool StripeChargeEnabled { get; set; }
    public bool StripePayoutsEnabled { get; set; }
    public ICollection<TenantMembership> Memberships { get; set; } = new List<TenantMembership>();
    public ICollection<TenantInvitation> Invitations { get; set; } = new List<TenantInvitation>();
}