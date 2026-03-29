using IdentityService.Api.Filters.Attributes;
using IdentityService.Application.Commands.Tenant;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Common.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Common.Constants;

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
    
    [HttpPatch("{membershipPublicId}/role")]
    [RequireTokenType(JwtTokenType.TenantAccessToken)]
    public async Task<IActionResult> ChangeTenantMemberRoleAsync(
        string membershipPublicId,
        [FromBody] ChangeTenantMemberRoleRequest request)
    {
        var jwtParseResult = HttpContext.GetHttpHeaderJwtParseResult();
        if (jwtParseResult == null)
        {
            return Unauthorized("Request without valid token");
        }

        var command = new ChangeTenantMemberRoleCommand(
            jwtParseResult.TenantPublicId,
            membershipPublicId,
            jwtParseResult.UserPublicId,
            request.Role
        );

        var result = await mediator.Send(command);
        return result.ToActionResult();
    }
}