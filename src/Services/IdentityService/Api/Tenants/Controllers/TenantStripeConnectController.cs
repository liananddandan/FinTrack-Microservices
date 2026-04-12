using IdentityService.Api.Common.Extensions;
using IdentityService.Api.Common.Filters.Attributes;
using IdentityService.Application.Tenants.Commands;
using IdentityService.Application.Tenants.Queries;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Common.Constants;
using SharedKernel.Common.DTOs.Auth;

namespace IdentityService.Api.Tenants.Controllers;

[ApiController]
[Route("api/tenant/stripe-connect")]
public class TenantStripeConnectController(IMediator mediator,
    ILogger<TenantStripeConnectController> logger) : ControllerBase
{
    [HttpGet("status")]
    [RequireTokenType(JwtTokenType.TenantAccessToken)]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken)
    {
        var authResult = await HttpContext.AuthenticateAsync();

        logger.LogInformation(
            "AuthenticateAsync result: Succeeded={Succeeded}, Failure={Failure}, PrincipalClaims={Claims}",
            authResult.Succeeded,
            authResult.Failure?.Message,
            string.Join(", ", authResult.Principal?.Claims.Select(x => $"{x.Type}={x.Value}") ?? []));
        
        var tenantPublicId = GetTenantPublicId();

        var result = await mediator.Send(
            new GetTenantStripeConnectStatusQuery(tenantPublicId),
            cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("onboarding-link")]
    [RequireTokenType(JwtTokenType.TenantAccessToken)]
    public async Task<IActionResult> CreateOrResumeOnboarding(CancellationToken cancellationToken)
    {
        var tenantPublicId = GetTenantPublicId();

        var result = await mediator.Send(
            new CreateTenantStripeOnboardingLinkCommand(tenantPublicId),
            cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid GetTenantPublicId()
    {
        var jwtParseResult = HttpContext.GetHttpHeaderJwtParseResult();

        if (jwtParseResult == null)
        {
            throw new UnauthorizedAccessException("Request without valid token");
        }

        if (string.IsNullOrWhiteSpace(jwtParseResult.TenantPublicId))
        {
            throw new UnauthorizedAccessException("Tenant claim is missing.");
        }

        return Guid.Parse(jwtParseResult.TenantPublicId);
    }
}