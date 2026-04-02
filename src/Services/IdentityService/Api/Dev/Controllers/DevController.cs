using IdentityService.Api.Common.Filters.Attributes;
using IdentityService.Application.Dev.Abstractions;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Common.DTOs;
using SharedKernel.Contracts.Dev;

namespace IdentityService.Api.Dev.Controllers;

[ApiController]
[Route("api/dev/seed")]

public class DevController(
    IWebHostEnvironment environment,
    IDevSeedService devSeedService) : ControllerBase
{
    [HttpPost("identity")]
    [AllowAnonymousToken]
    public async Task<IActionResult> SeedIdentityAsync(
        [FromBody] DevIdentitySeedRequest request,
        CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        var seedResult = await devSeedService.SeedIdentityAsync(request, cancellationToken);

        return Ok(new ApiResponse<DevIdentitySeedResult>(
            "IDENTITY_SEED_SUCCESS",
            "Identity demo data seeded successfully.",
            seedResult));
    }
}