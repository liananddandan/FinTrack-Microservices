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
}