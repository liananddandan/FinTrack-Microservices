using GatewayService.DTOs;
using GatewayService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Common.DTOs;

namespace GatewayService.Controllers;

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
