using IdentityService.Application.Common.DTOs;

namespace IdentityService.Application.Common.Extensions;

public static class HttpContextExtensions
{
    public static JwtParseDto? GetHttpHeaderJwtParseResult(this HttpContext httpContext)
    {
        return httpContext.Items["JwtParseResult"] as JwtParseDto;
    }
    
    public static InvitationParseDto? GetHttpHeaderInviteParseResult(this HttpContext httpContext)
    {
        return httpContext.Items["InviteParseResult"] as InvitationParseDto;
    }
}