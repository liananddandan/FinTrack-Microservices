using GatewayService.Application.Dev.Abstractions;
using GatewayService.Application.Dev.Dtos;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Common.DTOs;

namespace GatewayService.Api.Dev.Controllers;

[ApiController]
[Route("api/dev")]
public class DevController(
    IWebHostEnvironment environment,
    IDevSeedOrchestrator devSeedOrchestrator) : ControllerBase
{
    [HttpPost("seed")]
    public async Task<IActionResult> SeedAsync(CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        var seedResult = await devSeedOrchestrator.SeedAsync(cancellationToken);

        return Ok(new ApiResponse<DevSeedResult>(
            "DEV_SEED_SUCCESS",
            "Demo data seeded successfully.",
            seedResult));
    }
}
