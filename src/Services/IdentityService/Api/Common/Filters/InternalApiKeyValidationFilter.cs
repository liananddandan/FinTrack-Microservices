using IdentityService.Api.Common.Filters.Attributes;
using IdentityService.Application.Common.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;

namespace IdentityService.Api.Common.Filters;

public class InternalApiKeyValidationFilter(
    IOptions<InternalApiOptions> options) : IAsyncAuthorizationFilter
{
    private const string HeaderName = "X-Internal-Key";
    private readonly InternalApiOptions _options = options.Value;

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var requiresInternalKey = context.ActionDescriptor.EndpointMetadata
            .OfType<RequireInternalApiKeyAttribute>()
            .Any();

        if (!requiresInternalKey)
        {
            return Task.CompletedTask;
        }

        var providedKey = context.HttpContext.Request.Headers[HeaderName].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(providedKey))
        {
            context.Result = new JsonResult(new ApiResponse<object>(
                ResultCodes.Forbidden,
                "Missing internal API key.",
                null))
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };

            return Task.CompletedTask;
        }

        if (!string.Equals(providedKey, _options.ApiKey, StringComparison.Ordinal))
        {
            context.Result = new JsonResult(new ApiResponse<object>(
                ResultCodes.Forbidden,
                "Invalid internal API key.",
                null))
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };

            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}