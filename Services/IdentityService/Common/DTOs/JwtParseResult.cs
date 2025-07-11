using IdentityService.Common.Status;

namespace IdentityService.Common.DTOs;

public class JwtParseResult : JwtClaimSource
{
    public required JwtTokenType TokenType { get; set; }
}