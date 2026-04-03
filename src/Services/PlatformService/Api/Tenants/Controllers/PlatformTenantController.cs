using MediatR;
using Microsoft.AspNetCore.Mvc;
using PlatformService.Api.Extensions;
using PlatformService.Application.Tenants.Queries;

namespace PlatformService.Api.Tenants.Controllers;

[ApiController]
[Route("api/platform/tenants")]
public class PlatformTenantController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllPlatformTenantsQuery(), cancellationToken);
        return result.ToActionResult();
    }
}