using IdentityService.Api.Filters.Attributes;
using IdentityService.Application.Commands.Tenant;
using IdentityService.Application.Common.Extensions;
using IdentityService.Application.Common.Status;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("api/tenant/members")]
public class TenantMembersController(IMediator mediator) : ControllerBase
{
    [HttpDelete("{membershipPublicId}")]
    [RequireTokenType(JwtTokenType.TenantAccessToken)]
    public async Task<IActionResult> RemoveMember(string membershipPublicId)
    {
        var jwt = HttpContext.GetHttpHeaderJwtParseResult();

        if (jwt == null)
        {
            return Unauthorized();
        }

        var command = new RemoveTenantMemberCommand(
            membershipPublicId,
            jwt.TenantPublicId,
            jwt.UserPublicId
        );

        var result = await mediator.Send(command);

        return result.ToActionResult();
    }
}