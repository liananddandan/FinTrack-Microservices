namespace IdentityService.Common.DTOs;

public class InvitationClaimSource
{
    public required string InvitationPublicId { get; set; }
    public required string InvitationVersion { get; set; }
}