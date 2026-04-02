using IdentityService.Api.Common.Extensions;
using IdentityService.Api.Common.Filters.Attributes;
using IdentityService.Application.Common.Extensions;
using IdentityService.Application.Tenants.Commands;
using IdentityService.Application.Tenants.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Common.Constants;

namespace IdentityService.Api.Tenants.Controllers;

[ApiController]
[Route("api/tenant")]
public class TenantController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymousToken]
    public async Task<IActionResult> RegisterAsync(RegisterTenantCommand command)
    {
        var result = await mediator.Send(command);
        return result.ToActionResult();
    }
    
    [HttpGet("members")]
    [RequireTokenType(JwtTokenType.TenantAccessToken)]
    public async Task<IActionResult> GetTenantMembersAsync()
    {
        var jwtParseResult = HttpContext.GetHttpHeaderJwtParseResult();
        if (jwtParseResult == null)
        {
            return Unauthorized("Request without valid token");
        }

        var command = new GetTenantMembersCommand(jwtParseResult.TenantPublicId);
        var result = await mediator.Send(command);
        return result.ToActionResult();
    }
    
    [HttpGet("context")]
    [AllowAnonymousToken]
    public async Task<IActionResult> GetTenantContextAsync(
        CancellationToken cancellationToken)
    {
        var host = Request.GetOriginalHost();

        var result = await mediator.Send(
            new GetTenantContextQuery(host),
            cancellationToken);

        return result.ToActionResult();
    }
}