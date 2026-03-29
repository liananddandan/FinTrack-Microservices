using IdentityService.Api.Filters.Attributes;
using IdentityService.Application.Commands;
using IdentityService.Application.Commands.Account;
using IdentityService.Application.Commands.Tenant;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Common.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Common.Constants;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("api/account")]
public class AccountController(IMediator mediator) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymousToken]
    public async Task<IActionResult> UserLoginAsync(UserLoginCommand request)
    {
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }
    
    [HttpGet("me")]
    [RequireTokenType(JwtTokenType.AccountAccessToken)]
    public async Task<IActionResult> GetUsersInfoInTenantAsync()
    {
        var jwtParseResult = HttpContext.GetHttpHeaderJwtParseResult();
        if (jwtParseResult == null)
        {
            return Unauthorized("Request without valid token");
        }

        var command = new FetchUserInfoCommand(jwtParseResult.UserPublicId);
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

        var command = new RefreshUserJwtTokenCommand(
            jwtParseResult.UserPublicId,
            jwtParseResult.JwtVersion
        );
        var result = await mediator.Send(command);
        return result.ToActionResult();
    }
    
    [HttpPost("register")]
    [AllowAnonymousToken]
    public async Task<IActionResult> RegisterUserAsync(RegisterUserCommand command)
    {
        var result = await mediator.Send(command);
        return result.ToActionResult();
    }
    
    [HttpPost("select-tenant")]
    [RequireTokenType(JwtTokenType.AccountAccessToken)]
    public async Task<IActionResult> SelectTenantAsync([FromBody] SelectTenantRequest request)
    {
        var jwtParseResult = HttpContext.GetHttpHeaderJwtParseResult();
        if (jwtParseResult == null)
        {
            return Unauthorized("Request without valid token");
        }

        var command = new SelectTenantCommand(
            jwtParseResult.UserPublicId,
            request.TenantPublicId
        );

        var result = await mediator.Send(command);
        return result.ToActionResult();
    }
    
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

    [HttpPost("reset-password")]
    public async Task<IActionResult> UserResetPasswordAsync(ChangePasswordRequest request)
    {
        var jwtParseResult = HttpContext.GetHttpHeaderJwtParseResult();
        if (jwtParseResult == null)
        {
            return Unauthorized("Request without valid token");
        }

        var command = new SetUserPasswordCommand(jwtParseResult.UserPublicId, request.OldPassword, request.NewPassword, true);
        var result = await mediator.Send(command);
        return result.ToActionResult();
    }
}