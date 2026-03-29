using SharedKernel.Common.Constants;

namespace IdentityService.Application.Common.DTOs;

public class InvitationParseResult
{
    public required string InvitationPublicId { get; set; }
    public required string InvitationVersion { get; set; }
    public JwtTokenType TokenType { get; set; }
}