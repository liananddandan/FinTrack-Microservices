using IdentityService.Api.Common.Extensions;
using IdentityService.Api.Common.Filters.Attributes;
using IdentityService.Api.Tenants.Contracts;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Common.Extensions;
using IdentityService.Application.Tenants.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Common.Constants;

namespace IdentityService.Api.Tenants.Controllers;

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