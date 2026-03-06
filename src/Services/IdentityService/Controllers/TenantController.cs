using IdentityService.Commands;
using IdentityService.Common.DTOs;
using IdentityService.Common.Extensions;
using IdentityService.Common.Status;
using IdentityService.Filters.Attributes;
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

        var command = new InviteUserCommand(request.Emails, jwtParseResult.UserPublicId, jwtParseResult.TenantPublicId, jwtParseResult.UserRoleInTenant);
        var result = await mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpGet("receive-invite")]
    [RequireTokenType(JwtTokenType.InvitationToken)]
    public async Task<IActionResult> ReceiveInviteAsync()
    {
        var inviteParseResult = HttpContext.GetHttpHeaderInviteParseResult();
        if (inviteParseResult == null)
        {
            return Unauthorized("Request Without valid token");
        }

        var command = new ReceiveInviteCommand(inviteParseResult.InvitationPublicId);
        var result = await mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsersAsync()
    {
        var jwtParseResult = HttpContext.GetHttpHeaderJwtParseResult();
        if (jwtParseResult == null)
        {
            return Unauthorized("Request Without valid token");
        }
        var command = new GetUsersInTenantCommand(jwtParseResult.UserPublicId, jwtParseResult.TenantPublicId, jwtParseResult.UserRoleInTenant);
        var result = await mediator.Send(command);
        return result.ToActionResult();
    }
}