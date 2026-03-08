using IdentityService.Api.Filters.Attributes;
using IdentityService.Application.Commands;
using IdentityService.Application.Commands.Account;
using IdentityService.Application.Common.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("api/internal/account")]
public class AccountInternalController(IMediator mediator) : ControllerBase
{
    [HttpGet("{userPublicId}")]
    [AllowAnonymousToken]
    public async Task<IActionResult> GetUserInfoByPublicIdAsync([FromRoute]string userPublicId)
    {
        var command = new FetchUserInfoCommand(userPublicId);
        var result = await mediator.Send(command);
        return result.ToActionResult();
    }
}