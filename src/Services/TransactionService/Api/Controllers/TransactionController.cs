using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Common.DTOs.Auth;
using TransactionService.Application.Commands;
using TransactionService.Application.Common.DTOs;
using TransactionService.Application.Common.Extensions;
using TransactionService.Application.Queries;

namespace TransactionService.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/transactions")]
public class TransactionsController(IMediator mediator) : ControllerBase
{
    [HttpPost("donations")]
    public async Task<IActionResult> CreateDonationAsync(
        [FromBody] CreateDonationRequest request,
        CancellationToken cancellationToken)
    {
        var tenantPublicId = User.FindFirst(JwtClaimNames.Tenant)?.Value;
        var userPublicId = User.FindFirst(JwtClaimNames.UserId)?.Value;
        Console.WriteLine(
            $"Donation endpoint claims. tenant={tenantPublicId}, sub={userPublicId}");
        
        if (string.IsNullOrWhiteSpace(tenantPublicId) ||
            string.IsNullOrWhiteSpace(userPublicId))
        {
            return Unauthorized();
        }

        var result = await mediator.Send(
            new CreateDonationCommand(
                tenantPublicId,
                userPublicId,
                request.Title,
                request.Description,
                request.Amount,
                request.Currency),
            cancellationToken);

        return result.ToActionResult();
    }
    
    [HttpGet("my")]
    public async Task<IActionResult> GetMyTransactionsAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var tenantPublicId = User.FindFirst(JwtClaimNames.Tenant)?.Value;
        var userPublicId = User.FindFirst(JwtClaimNames.UserId)?.Value;
        
        if (string.IsNullOrWhiteSpace(tenantPublicId) ||
            string.IsNullOrWhiteSpace(userPublicId))
        {
            return Unauthorized();
        }

        var result = await mediator.Send(
            new GetMyTransactionsQuery(
                tenantPublicId,
                userPublicId,
                pageNumber,
                pageSize),
            cancellationToken);

        return result.ToActionResult();
    }
}