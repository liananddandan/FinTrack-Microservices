using Microsoft.AspNetCore.Mvc;
using SharedKernel.Common.DTOs;
using SharedKernel.Contracts.Dev;
using TransactionService.Application.Dev.Abstractions;

namespace TransactionService.Api.Dev.Controllers;

[ApiController]
[Route("api/dev/seed")]
public class DevController(
    IWebHostEnvironment environment,
    IDevSeedService devSeedService) : ControllerBase
{
    [HttpPost("menu-and-orders")]
    public async Task<IActionResult> SeedMenuAndOrdersAsync(
        [FromBody] DevTransactionSeedRequest request,
        CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        var seedResult = await devSeedService.SeedMenuAndOrdersAsync(request, cancellationToken);

        return Ok(new ApiResponse<DevTransactionSeedResult>(
            "MENU_ORDER_SEED_SUCCESS",
            "Menu and order demo data seeded successfully.",
            seedResult));
    }
}