using IdentityService.Application.Common.Status;
using IdentityService.Domain.Enums;

namespace IdentityService.Domain.Entities;

public class TenantInvitation
{
    public long Id { get; set; }
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public required string Email { get; set; }
    
    public long TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public TenantRole Role { get; set; } = TenantRole.Member;

    public InvitationStatus Status {get; set;}
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public DateTime? AcceptedAt {get; set;}
    public DateTime ExpiredAt {get; set;}
    public int Version {get; set;}
    public long CreatedByUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
}