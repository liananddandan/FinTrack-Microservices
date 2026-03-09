using Microsoft.AspNetCore.Mvc;
using SharedKernel.Common.DTOs;
using TransactionService.Application.DTOs;
using TransactionService.Application.Services.Interfaces;

namespace TransactionService.Api.Controllers;

[ApiController]
[Route("api/dev/seed")]
public class DevController(
    IWebHostEnvironment environment,
    IDevSeedService devSeedService) : ControllerBase
{
    [HttpPost("transactions")]
    public async Task<IActionResult> SeedTransactionsAsync(
        [FromBody] DevTransactionSeedRequest request,
        CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        var seedResult = await devSeedService.SeedTransactionsAsync(request, cancellationToken);

        return Ok(new ApiResponse<DevTransactionSeedResult>(
            "TRANSACTION_SEED_SUCCESS",
            "Transaction demo data seeded successfully.",
            seedResult));
    }
}
