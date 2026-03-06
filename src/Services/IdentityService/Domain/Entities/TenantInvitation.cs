using IdentityService.Common.Status;

namespace IdentityService.Domain.Entities;

public class TenantInvitation
{
    public long Id { get; set; }
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public required string Email { get; set; }
    public required string TenantPublicId {get; set;}
    public required string Role { get; set; }
    public InvitationStatus Status {get; set;}
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public DateTime? AcceptedAt {get; set;}
    public DateTime ExpiredAt {get; set;}
    public int Version {get; set;}
    public required string CreatedBy {get; set;}
}