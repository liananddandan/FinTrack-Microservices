using IdentityService.Common.Status;

namespace IdentityService.Common.DTOs;

public class InvitationParseResult
{
    public required string InvitationPublicId { get; set; }
    public required string InvitationVersion { get; set; }
    public JwtTokenType TokenType { get; set; }
}