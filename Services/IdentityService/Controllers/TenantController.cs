using IdentityService.Commands;
using IdentityService.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync(RegisterTenantCommand command)
    {
        var result = await mediator.Send(command);
        return result.ToActionResult();
    }
}