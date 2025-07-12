using IdentityService.Common.Results;
using IdentityService.Common.Status;
using IdentityService.Filters.Attributes;
using IdentityService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SharedKernel.Common.DTOs;

namespace IdentityService.Filters;

public class GlobalJwtTokenValidationFilter(IJwtTokenService jwtTokenService) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousTokenAttribute>().Any())
        {
            return;
        }

        var token = context.HttpContext.Request.Headers["Authorization"]
            .FirstOrDefault()?.Replace("Bearer ", "");

        if (string.IsNullOrWhiteSpace(token))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var result = await jwtTokenService.ParseJwtTokenAsync(token);
        if (!result.Success)
        {
            context.Result = new JsonResult(new ApiResponse<object>(
                result.Code ?? ResultCodes.InternalError,
                result.Message ?? "",
                null
            ))
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
            return;
        }

        if (result.Data == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var requiredType =
            context.ActionDescriptor.EndpointMetadata
                .OfType<RequireTokenTypeAttribute>().FirstOrDefault()?
                .TokenType ?? JwtTokenType.AccessToken;
        
        if (result.Data.TokenType != requiredType)
        {
            context.Result = new ForbidResult();
            return;
        }
        
        context.HttpContext.Items["JwtParseResult"] = result.Data;
    }
}