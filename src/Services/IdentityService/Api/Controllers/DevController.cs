using IdentityService.Api.Filters.Attributes;
using IdentityService.Application.Abstractions;
using IdentityService.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Common.DTOs;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("api/dev/seed")]
public class DevController(
    IWebHostEnvironment environment,
    IDevSeedService devSeedService) : ControllerBase
{
    [HttpPost("identity")]
    [AllowAnonymousToken]
    public async Task<IActionResult> SeedIdentityAsync(CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        var seedResult = await devSeedService.SeedIdentityAsync(cancellationToken);

        return Ok(new ApiResponse<DevIdentitySeedResult>(
            "IDENTITY_SEED_SUCCESS",
            "Identity demo data seeded successfully.",
            seedResult));
    }
}
