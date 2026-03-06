using IdentityService.Application.Common.Status;

namespace IdentityService.Application.Common.DTOs;

public class JwtParseResult : JwtClaimSource
{
    public required JwtTokenType TokenType { get; set; }
}