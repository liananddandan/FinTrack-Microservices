using IdentityService.Common.DTOs;

namespace IdentityService.Common.Extensions;

public static class HttpContextExtensions
{
    public static JwtParseResult? GetJwtParseResult(this HttpContext httpContext)
    {
        return httpContext.Items["JwtParseResult"] as JwtParseResult;
    }
}