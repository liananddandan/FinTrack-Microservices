using IdentityService.Application.Common.DTOs;

namespace IdentityService.Application.Common.Extensions;

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