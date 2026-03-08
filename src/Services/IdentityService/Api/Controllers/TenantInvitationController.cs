using IdentityService.Api.Filters.Attributes;
using IdentityService.Application.Commands.Tenant;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Common.Extensions;
using IdentityService.Application.Common.Status;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("api/tenant/invitations")]
public class TenantInvitationController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [RequireTokenType(JwtTokenType.TenantAccessToken)]
    public async Task<IActionResult> InviteMemberAsync(
        [FromBody] InviteMemberRequest request)
    {
        var jwtParseResult = HttpContext.GetHttpHeaderJwtParseResult();

        if (jwtParseResult == null)
        {
            return Unauthorized();
        }

        var command = new CreateTenantInvitationCommand(
            jwtParseResult.TenantPublicId,
            request.Email,
            request.Role,
            jwtParseResult.UserPublicId
        );

        var result = await mediator.Send(command);

        return result.ToActionResult();
    }
    
    
    [HttpGet("resolve")]
    [RequireTokenType(JwtTokenType.InvitationToken)]
    public async Task<IActionResult> ResolveInvitationAsync()
    {
        var invitationParseResult = HttpContext.GetHttpHeaderInviteParseResult();
        if (invitationParseResult == null)
        {
            return Unauthorized("Request without valid invitation token");
        }

        var command = new ResolveTenantInvitationCommand(
            invitationParseResult.InvitationPublicId,
            invitationParseResult.InvitationVersion
        );

        var result = await mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpPost("accept")]
    [RequireTokenType(JwtTokenType.InvitationToken)]
    public async Task<IActionResult> AcceptInvitationAsync()
    {
        var invitationParseResult = HttpContext.GetHttpHeaderInviteParseResult();
        if (invitationParseResult == null)
        {
            return Unauthorized("Request without valid invitation token");
        }

        var command = new AcceptTenantInvitationCommand(
            invitationParseResult.InvitationPublicId,
            invitationParseResult.InvitationVersion
        );

        var result = await mediator.Send(command);
        return result.ToActionResult();
    }
    
    [HttpGet]
    [RequireTokenType(JwtTokenType.TenantAccessToken)]
    public async Task<IActionResult> GetTenantInvitationsAsync()
    {
        var jwtParseResult = HttpContext.GetHttpHeaderJwtParseResult();
        if (jwtParseResult == null)
        {
            return Unauthorized();
        }

        var command = new GetTenantInvitationsCommand(jwtParseResult.TenantPublicId);
        var result = await mediator.Send(command);
        return result.ToActionResult();
    }
    
    [HttpPost("{invitationPublicId}/resend")]
    [RequireTokenType(JwtTokenType.TenantAccessToken)]
    public async Task<IActionResult> ResendInvitationAsync(string invitationPublicId)
    {
        var jwtParseResult = HttpContext.GetHttpHeaderJwtParseResult();
        if (jwtParseResult == null)
        {
            return Unauthorized();
        }

        var command = new ResendTenantInvitationCommand(
            jwtParseResult.TenantPublicId,
            invitationPublicId
        );

        var result = await mediator.Send(command);
        return result.ToActionResult();
    }
}