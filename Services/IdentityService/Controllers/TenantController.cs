using IdentityService.Commands;
using IdentityService.Common.DTOs;
using IdentityService.Common.Extensions;
using IdentityService.Common.Results;
using IdentityService.Filters.Attributes;
using IdentityService.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers;

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

    [HttpPost("invite")]
    public async Task<IActionResult> InviteUserAsync(InviteUserRequest request)
    {
        if (request.Emails.Count <= 0)
        {
            return BadRequest("Invalid request without email address");
        }
        var jwtParseResult = HttpContext.GetHttpHeaderJwtParseResult();
        if (jwtParseResult == null)
        {
            return Unauthorized("Request Without valid token");
        }
        
        var command = new InviteUserCommand()
        {
            AdminUserPublicId = jwtParseResult.UserPublicId,
            AdminJwtVersion = jwtParseResult.JwtVersion,
            TenantPublicid = jwtParseResult.TenantPublicId,
            AdminRoleInTenant = jwtParseResult.UserRoleInTenant,
            Emails = request.Emails
        };
        var result = await mediator.Send(command);
        return result.ToActionResult();
    }
}