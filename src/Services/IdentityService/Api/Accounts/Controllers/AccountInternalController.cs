using IdentityService.Application.Accounts.Commands;
using IdentityService.Application.Common.Extensions;
using IdentityService.Application.Common.Filters.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Accounts.Controllers;

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