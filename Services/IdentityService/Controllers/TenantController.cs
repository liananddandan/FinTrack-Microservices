using IdentityService.Commands;
using IdentityService.Common.Extensions;
using IdentityService.Common.Results;
using IdentityService.Filters.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/tenant")]
public class TenantController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymousToken]
    public async Task<IActionResult> RegisterAsync(RegisterTenantCommand command)
    {
        var result = await mediator.Send(command);
        return result.ToActionResult();
    }
}