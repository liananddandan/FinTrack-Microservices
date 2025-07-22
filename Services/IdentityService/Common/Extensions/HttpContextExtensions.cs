using IdentityService.Common.DTOs;

namespace IdentityService.Common.Extensions;

public static class HttpContextExtensions
{
    public static JwtParseResult? GetHttpHeaderJwtParseResult(this HttpContext httpContext)
    {
        return httpContext.Items["JwtParseResult"] as JwtParseResult;
    }
    
    public static InvitationParseResult? GetHttpHeaderInviteParseResult(this HttpContext httpContext)
    {
        return httpContext.Items["InviteParseResult"] as InvitationParseResult;
    }
}