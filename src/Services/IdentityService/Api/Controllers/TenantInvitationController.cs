using IdentityService.Application.Commands.Tenant;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Common.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("api/tenant/invitations")]
public class TenantInvitationController(IMediator mediator) : ControllerBase
{
    [HttpPost]
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
}