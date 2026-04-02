
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PlatformService.Api.Extensions;
using PlatformService.Api.Tenants.Contracts;
using PlatformService.Application.Tenants.Commands;
using PlatformService.Application.Tenants.Queries;

namespace PlatformService.Api.Tenants.Controllers;

[ApiController]
[Route("api/platform/tenant-domains")]
public class TenantDomainMappingController(IMediator mediator) : ControllerBase
{
    [HttpGet("by-tenant/{tenantPublicId:guid}")]
    public async Task<IActionResult> GetByTenantAsync(
        Guid tenantPublicId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetTenantDomainMappingsQuery(tenantPublicId),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateTenantDomainMappingRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateTenantDomainMappingCommand(
                request.TenantPublicId,
                request.Host,
                request.DomainType,
                request.IsPrimary,
                request.IsActive),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPut("{domainPublicId:guid}")]
    public async Task<IActionResult> UpdateAsync(
        Guid domainPublicId,
        [FromBody] UpdateTenantDomainMappingRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateTenantDomainMappingCommand(
                domainPublicId,
                request.Host,
                request.DomainType,
                request.IsPrimary,
                request.IsActive),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPatch("{domainPublicId:guid}/active")]
    public async Task<IActionResult> SetActiveAsync(
        Guid domainPublicId,
        [FromBody] SetTenantDomainMappingActiveRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new SetTenantDomainMappingActiveCommand(
                domainPublicId,
                request.IsActive),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpDelete("{domainPublicId:guid}")]
    public async Task<IActionResult> DeleteAsync(
        Guid domainPublicId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new DeleteTenantDomainMappingCommand(domainPublicId),
            cancellationToken);

        return result.ToActionResult();
    }
}