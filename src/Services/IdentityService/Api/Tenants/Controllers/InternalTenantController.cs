using IdentityService.Api.Common.Filters.Attributes;
using IdentityService.Application.Common.Extensions;
using IdentityService.Application.Tenants.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Tenants.Controllers;

[ApiController]
[Route("internal/tenants")]
[RequireInternalApiKey]
[AllowAnonymousToken]
public class InternalTenantController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllTenantsQuery(), cancellationToken);
        return result.ToActionResult();
    }
    
    [HttpGet("{tenantPublicId:guid}/stripe-connect/status")]
    [RequireInternalApiKey]
    public async Task<IActionResult> GetStripeConnectStatus(
        Guid tenantPublicId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetTenantStripeConnectStatusQuery(tenantPublicId),
            cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }
}