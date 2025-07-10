using System.Web;
using IdentityService.Commands;
using IdentityService.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController(IMediator mediator)
{
    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmAccountEmailAsync(string userId, string token)
    {
        var decodedToken = Uri.UnescapeDataString(token);
        var decodeUserId = Uri.UnescapeDataString(userId);
        ConfirmAccountEmailCommand request = new(decodeUserId, decodedToken);
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpPost("login")]
    public async Task<IActionResult> UserLoginAsync(UserLoginCommand request)
    {
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }
}