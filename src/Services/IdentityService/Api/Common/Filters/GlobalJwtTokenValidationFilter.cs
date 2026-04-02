using IdentityService.Api.Common.Filters.Attributes;
using IdentityService.Application.Accounts.Abstractions;
using IdentityService.Application.Common.Abstractions;
using IdentityService.Application.Tenants.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SharedKernel.Common.Constants;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;

namespace IdentityService.Api.Common.Filters;

public class GlobalJwtTokenValidationFilter(
    IJwtTokenService jwtTokenService,
    IUserDomainService userDomainService,
    ITenantInvitationService tenantInvitationService)
    : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousTokenAttribute>().Any())
        {
            return;
        }

        var authorizationHeader = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (authorizationHeader.StartsWith("Invite ", StringComparison.OrdinalIgnoreCase))
        {
            await HandleInvitationTokenAsync(context, authorizationHeader["Invite ".Length..]);
            return;
        }

        if (authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            await HandleBearerTokenAsync(context, authorizationHeader["Bearer ".Length..]);
            return;
        }

        context.Result = new UnauthorizedResult();
    }

    private async Task HandleInvitationTokenAsync(
        AuthorizationFilterContext context,
        string token)
    {
        var result = await jwtTokenService.ParseInvitationTokenAsync(token);

        if (!result.Success || result.Data == null)
        {
            context.Result = BuildUnauthorizedErrorResult(result);
            return;
        }

        var requiredTokenType = GetRequiredTokenType(context);
        if (result.Data.TokenType != requiredTokenType)
        {
            context.Result = BuildForbiddenErrorResult("Token type not supported");
            return;
        }

        var invitationParsedResult = result.Data;

        var invitation = await tenantInvitationService
            .GetTenantInvitationByPublicIdAsync(invitationParsedResult.InvitationPublicId);

        if (!invitation.Success || invitation.Data == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!int.TryParse(invitationParsedResult.InvitationVersion, out var version) ||
            version != invitation.Data.Version)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        context.HttpContext.Items["InviteParseResult"] = invitationParsedResult;
    }

    private async Task HandleBearerTokenAsync(
        AuthorizationFilterContext context,
        string token)
    {
        var result = await jwtTokenService.ParseJwtTokenAsync(token);

        if (!result.Success || result.Data == null)
        {
            context.Result = BuildUnauthorizedErrorResult(result);
            return;
        }

        var requiredTokenType = GetRequiredTokenType(context);
        if (result.Data.TokenType != requiredTokenType)
        {
            context.Result = BuildForbiddenErrorResult("Token type not supported");
            return;
        }

        var jwtParseResult = result.Data;

        var user = await userDomainService.GetUserByPublicIdAsync(jwtParseResult.UserPublicId);
        if (user == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!long.TryParse(jwtParseResult.JwtVersion, out var version) ||
            version != user.JwtVersion)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        context.HttpContext.Items["JwtParseResult"] = jwtParseResult;
    }

    private IActionResult BuildUnauthorizedErrorResult<T>(ServiceResult<T> result)
    {
        return new JsonResult(new ApiResponse<object>(
            result.Code ?? ResultCodes.InternalError,
            result.Message ?? string.Empty,
            null))
        {
            StatusCode = StatusCodes.Status401Unauthorized
        };
    }

    private IActionResult BuildForbiddenErrorResult(string message)
    {
        return new JsonResult(new ApiResponse<object>(
            ResultCodes.Forbidden,
            message,
            null))
        {
            StatusCode = StatusCodes.Status403Forbidden
        };
    }

    private JwtTokenType GetRequiredTokenType(AuthorizationFilterContext context)
    {
        return context.ActionDescriptor.EndpointMetadata
            .OfType<RequireTokenTypeAttribute>()
            .FirstOrDefault()?.TokenType
               ?? JwtTokenType.AccountAccessToken;
    }
}