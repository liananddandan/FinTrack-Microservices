using System.Web;
using IdentityService.Commands;
using IdentityService.Common.DTOs;
using IdentityService.Common.Extensions;
using IdentityService.Common.Results;
using IdentityService.Common.Status;
using IdentityService.Filters.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController(IMediator mediator) : ControllerBase
{
    [HttpGet("confirm-email")]
    [AllowAnonymousToken]
    public async Task<IActionResult> ConfirmAccountEmailAsync(string userId, string token)
    {
        var decodedToken = Uri.UnescapeDataString(token);
        var decodeUserId = Uri.UnescapeDataString(userId);
        ConfirmAccountEmailCommand request = new(decodeUserId, decodedToken);
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpPost("login")]
    [AllowAnonymousToken]
    public async Task<IActionResult> UserLoginAsync(UserLoginCommand request)
    {
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpPost("set-password")]
    [RequireTokenType(JwtTokenType.FirstLoginToken)]
    public async Task<IActionResult> UserFirstTimeChangePasswordAsync(ChangePasswordRequest request)
    {
        var jwtParseResult = HttpContext.GetJwtParseResult();
        if (jwtParseResult == null)
        {
            return Unauthorized("Request without valid token");
        }

        var command = new ChangeUserPasswordCommand(jwtParseResult.UserPublicId,
            jwtParseResult.JwtVersion, request.OldPassword, request.NewPassword);
        var result = await mediator.Send(command);
        return result.ToActionResult();
    }
}