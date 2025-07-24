using IdentityService.Commands;
using IdentityService.Filters.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Extensions;

namespace IdentityService.Controllers;

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