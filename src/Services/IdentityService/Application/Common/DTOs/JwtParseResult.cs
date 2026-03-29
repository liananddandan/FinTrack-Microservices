using IdentityService.Application.Common.Status;
using SharedKernel.Common.Constants;

namespace IdentityService.Application.Common.DTOs;

public class JwtParseResult : JwtClaimSource
{
    public required JwtTokenType TokenType { get; set; }
}