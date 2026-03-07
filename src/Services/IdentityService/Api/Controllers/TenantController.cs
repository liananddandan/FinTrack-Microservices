using IdentityService.Api.Filters.Attributes;
using IdentityService.Application.Commands;
using IdentityService.Application.Commands.Tenant;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Common.Extensions;
using IdentityService.Application.Common.Status;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers;

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