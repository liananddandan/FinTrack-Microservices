using GatewayService.Application.Common.Options;
using Microsoft.Extensions.Options;
using SharedKernel.Common.Constants;
using SharedKernel.Common.DTOs.Auth;
using StackExchange.Redis;

namespace GatewayService.Application.Middlewares;

public class BasicJwtTokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly List<string> _authWhiteList;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<BasicJwtTokenValidationMiddleware> _logger;

    public BasicJwtTokenValidationMiddleware(
        RequestDelegate next,
        IOptions<AuthenticationOptions> authOptions,
        IConnectionMultiplexer redis,
        ILogger<BasicJwtTokenValidationMiddleware> logger)
    {
        _next = next;
        _authWhiteList = authOptions.Value.AuthWhiteList;
        _redis = redis;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var requestPath = context.Request.Path;

        if (requestPath.StartsWithSegments("/api/swagger") ||
            requestPath.StartsWithSegments("/api/openapi") ||
            requestPath == "/favicon.ico")
        {
            await _next(context);
            return;
        }
        
        var path = requestPath.Value ?? string.Empty;

        if (_authWhiteList.Any(p => MatchPath(p, path)))
        {
            _logger.LogDebug("Gateway auth skipped for whitelisted path: {Path}", path);
            await _next(context);
            return;
        }

        var token = context.Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("Gateway rejected request: missing Authorization header. Path={Path}", path);
            context.Response.Headers["X-Auth-Source"] = "Gateway";
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized, Invalid or expired token");
            return;
        }

        if (token.StartsWith("Invite ", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("Gateway passing invitation token downstream. Path={Path}", path);
            await _next(context);
            return;
        }

        if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogWarning("Gateway rejected request: unauthenticated principal. Path={Path}", path);
                context.Response.Headers["X-Auth-Source"] = "Gateway";
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized, Invalid or expired token");
                return;
            }

            var userPublicId = context.User.FindFirst(JwtClaimNames.UserId)?.Value;
            var jwtVersion = context.User.FindFirst(JwtClaimNames.JwtVersion)?.Value;
            var tokenType = context.User.FindFirst(JwtClaimNames.TokenType)?.Value;

            var tenantPublicId = context.User.FindFirst(JwtClaimNames.Tenant)?.Value;
            var userRoleInTenant = context.User.FindFirst(JwtClaimNames.Role)?.Value;

            _logger.LogInformation(
                "Gateway token claims. Path={Path}, UserPublicId={UserPublicId}, TokenType={TokenType}, TenantPublicId={TenantPublicId}, Role={Role}, JwtVersion={JwtVersion}",
                path, userPublicId, tokenType, tenantPublicId, userRoleInTenant, jwtVersion);

            if (string.IsNullOrEmpty(userPublicId)
                || string.IsNullOrEmpty(jwtVersion)
                || string.IsNullOrEmpty(tokenType))
            {
                _logger.LogWarning(
                    "Gateway rejected request: missing required claims. Path={Path}, UserPublicId={UserPublicId}, TokenType={TokenType}, JwtVersion={JwtVersion}",
                    path, userPublicId, tokenType, jwtVersion);

                context.Response.Headers["X-Auth-Source"] = "Gateway";
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            var redisKey = $"{Constant.Redis.JwtVersionPrefix}{userPublicId}";
            var redisVersion = await _redis.GetDatabase().StringGetAsync(redisKey);

            _logger.LogInformation(
                "Gateway jwtVersion check. Path={Path}, UserPublicId={UserPublicId}, TokenJwtVersion={TokenJwtVersion}, RedisJwtVersion={RedisJwtVersion}",
                path, userPublicId, jwtVersion, redisVersion.ToString());

            if (!jwtVersion.Equals(redisVersion))
            {
                _logger.LogWarning(
                    "Gateway rejected request: token version mismatch. Path={Path}, UserPublicId={UserPublicId}, TokenJwtVersion={TokenJwtVersion}, RedisJwtVersion={RedisJwtVersion}",
                    path, userPublicId, jwtVersion, redisVersion.ToString());

                context.Response.Headers["X-Auth-Source"] = "Gateway";
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Token version mismatch.");
                return;
            }

            _logger.LogDebug(
                "Gateway auth passed. Path={Path}, UserPublicId={UserPublicId}, TokenType={TokenType}",
                path, userPublicId, tokenType);

            await _next(context);
            return;
        }

        _logger.LogWarning(
            "Gateway rejected request: unsupported Authorization header format. Path={Path}, RawHeader={Header}",
            path, token);

        context.Response.Headers["X-Auth-Source"] = "Gateway";
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