using System.Web;
using IdentityService.Commands;
using IdentityService.Common.DTOs;
using IdentityService.Common.Extensions;
using IdentityService.Common.Results;
using IdentityService.Common.Status;
using IdentityService.Filters.Attributes;
using MediatR;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/account")]
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
        var jwtParseResult = HttpContext.GetHttpHeaderJwtParseResult();
        if (jwtParseResult == null)
        {
            return Unauthorized("Request without valid token");
        }

        var command = new SetUserPasswordCommand(jwtParseResult.UserPublicId,
            jwtParseResult.JwtVersion,request.OldPassword, request.NewPassword, false);
        var result = await mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpPost("reset-password")]
    [RequireTokenType(JwtTokenType.AccessToken)]
    public async Task<IActionResult> UserResetPasswordAsync(ChangePasswordRequest request)
    {
        var jwtParseResult = HttpContext.GetHttpHeaderJwtParseResult();
        if (jwtParseResult == null)
        {
            return Unauthorized("Request without valid token");
        }
        var command = new SetUserPasswordCommand(jwtParseResult.UserPublicId, jwtParseResult.JwtVersion, 
            request.OldPassword, request.NewPassword, true);
        var result = await mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpGet("refresh-token")]
    [RequireTokenType(JwtTokenType.RefreshToken)]
    public async Task<IActionResult> RefreshUserJwtTokenAsync()
    {
        var jwtParseResult = HttpContext.GetHttpHeaderJwtParseResult();
        if (jwtParseResult == null)
        {
            return Unauthorized("Request without valid token");
        }
        var command = new RefreshUserJwtTokenCommand(jwtParseResult.UserPublicId, jwtParseResult.JwtVersion);
        var result = await mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpGet]
    public async Task<IActionResult> GetUserInfoAsync()
    {
        var jwtParseResult = HttpContext.GetHttpHeaderJwtParseResult();
        if (jwtParseResult == null)
        {
            return Unauthorized("Request without valid token");
        }
        var command = new FetchUserInfoCommand(jwtParseResult.UserPublicId, jwtParseResult.JwtVersion);
        var result = await mediator.Send(command);
        return result.ToActionResult();
    }
}