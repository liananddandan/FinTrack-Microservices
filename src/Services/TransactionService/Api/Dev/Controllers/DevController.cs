using Microsoft.AspNetCore.Mvc;
using SharedKernel.Common.DTOs;
using TransactionService.Api.Dev.Contracts;
using TransactionService.Application.Dev.Abstractions;

namespace TransactionService.Api.Dev.Controllers;

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
