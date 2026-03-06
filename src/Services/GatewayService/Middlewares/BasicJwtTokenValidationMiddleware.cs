using System.IdentityModel.Tokens.Jwt;
using System.Text;
using GatewayService.Common.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Common.Constants;
using SharedKernel.Common.Options;
using StackExchange.Redis;

namespace GatewayService.Middlewares;

public class BasicJwtTokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly List<string> _authWhiteList;
    private readonly IConnectionMultiplexer _redis;

    public BasicJwtTokenValidationMiddleware(RequestDelegate next, 
        IOptions<AuthenticationOptions> authOptions,
        IConnectionMultiplexer redis)
    {
        _next = next;
        _authWhiteList = authOptions.Value.AuthWhiteList;
        _redis = redis;
    }

    public async Task Invoke(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (_authWhiteList.Any(p => MatchPath(p, path)))
        {
            await _next(context);
            return;
        }

        var token = context.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(token))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized, Invalid or expired token");
            return;
        }
        
                
        if (token.StartsWith("Invite ", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            // standard jwt token
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized, Invalid or expired token");
                return;
            }
            
            var tenantPublicId = context.User.FindFirst("TenantPublicId")?.Value;
            var userPublicId = context.User.FindFirst("UserPublicId")?.Value;
            var jwtVersion = context.User.FindFirst("JwtVersion")?.Value;
            var userRoleInTenant = context.User.FindFirst("UserRoleInTenant")?.Value;
            if (string.IsNullOrEmpty(tenantPublicId) 
                || string.IsNullOrEmpty(userPublicId)
                || string.IsNullOrEmpty(userRoleInTenant)
                || string.IsNullOrEmpty(jwtVersion))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
            
            var redisVersion = await _redis.GetDatabase().StringGetAsync($"{Constant.Redis.JwtVersionPrefix}{userPublicId}");
            if (!jwtVersion.Equals(redisVersion))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Token version mismatch.");
                return;
            }
            
            await _next(context);
            return;
        }
        
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Unauthorized");
    }

    private bool MatchPath(string pattern, string path)
    {
        if (pattern.EndsWith("/*"))
        {
            return path.StartsWith(pattern.TrimEnd('*'));
        }
        return path.Equals(pattern, StringComparison.OrdinalIgnoreCase);
    }
}