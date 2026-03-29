using IdentityService.Application.Common.Status;
using SharedKernel.Common.Constants;

namespace IdentityService.Application.Common.DTOs;

public class JwtParseDto : JwtClaimSource
{
    public required JwtTokenType TokenType { get; set; }
}