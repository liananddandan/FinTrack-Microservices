using IdentityService.Application.Common.Extensions;
using IdentityService.Application.Common.Filters.Attributes;
using IdentityService.Application.Tenants.Commands;
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
}