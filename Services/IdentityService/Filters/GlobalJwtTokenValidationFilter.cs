using IdentityService.Common.Results;
using IdentityService.Common.Status;
using IdentityService.Filters.Attributes;
using IdentityService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;

namespace IdentityService.Filters;

public class GlobalJwtTokenValidationFilter(IJwtTokenService jwtTokenService,
    IUserDomainService userDomainService,
    ITenantInvitationService tenantInvitationService) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousTokenAttribute>().Any())
        {
            return;
        }

        var token = context.HttpContext.Request.Headers["Authorization"]
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(token))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!token.StartsWith("Bearer ") && !token.StartsWith("Invite "))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var inviteToken = token.StartsWith("Invite ");
        token = inviteToken ? token.Replace("Invite ", "") : token.Replace("Bearer ", "");

        if (inviteToken)
        {
            var result = await jwtTokenService.ParseInvitationTokenAsync(token);
            if (!result.Success || result.Data == null)
            {
                context.Result = BuildErrorResult(result);
                return;
            }
            if (result.Data.TokenType != GetRequiredTokenType(context))
            {
                context.Result = new ForbidResult("Token type not supported");
                return;
            }
            var invitationParsedResult = result.Data;
            var invitation =
                await tenantInvitationService
                    .GetTenantInvitationByPublicIdAsync(invitationParsedResult.InvitationPublicId);
            if (!invitation.Success || invitation.Data == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            if (!long.TryParse(invitationParsedResult.InvitationVersion, out var version)
                || version != invitation.Data.Version)
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            context.HttpContext.Items["InviteParseResult"] = invitationParsedResult;
        }
        else
        {
            var result = await jwtTokenService.ParseJwtTokenAsync(token);
            if (!result.Success || result.Data == null)
            {
                context.Result = BuildErrorResult(result);
                return;
            }
            if (result.Data.TokenType != GetRequiredTokenType(context))
            {
                context.Result = new ForbidResult("Token type not supported");
                return;
            }
            var jwtParseResult = result.Data;
            
            var user = await userDomainService.GetUserByPublicIdIncludeTenantAsync(result.Data.UserPublicId);
            if (user == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            if (!long.TryParse(jwtParseResult.JwtVersion, out var version)
                || version != user.JwtVersion)
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            context.HttpContext.Items["JwtParseResult"] = jwtParseResult;
        }
    }
    
    private IActionResult BuildErrorResult<T>(ServiceResult<T> result)
    {
        return new JsonResult(new ApiResponse<object>(
            result.Code ?? ResultCodes.InternalError,
            result.Message ?? "",
            null))
        {
            StatusCode = StatusCodes.Status401Unauthorized
        };
    }

    private JwtTokenType GetRequiredTokenType(AuthorizationFilterContext context)
    {
        return context.ActionDescriptor.EndpointMetadata
            .OfType<RequireTokenTypeAttribute>()
            .FirstOrDefault()?.TokenType ?? JwtTokenType.AccessToken;
    }
}