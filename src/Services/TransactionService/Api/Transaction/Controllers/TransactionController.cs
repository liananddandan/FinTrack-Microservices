using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Common.DTOs.Auth;
using TransactionService.Api.Transaction.Contracts;
using TransactionService.Application.Common.Extensions;
using TransactionService.Application.Transactions.Commands;
using TransactionService.Application.Transactions.Queries;

namespace TransactionService.Api.Transaction.Controllers;

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

    [HttpGet("{transactionPublicId}")]
    public async Task<IActionResult> GetTransactionDetailAsync(
        [FromRoute] string transactionPublicId,
        CancellationToken cancellationToken = default)
    {
        var tenantPublicId = User.FindFirst(JwtClaimNames.Tenant)?.Value;
        var userPublicId = User.FindFirst(JwtClaimNames.UserId)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(tenantPublicId) ||
            string.IsNullOrWhiteSpace(userPublicId))
        {
            return Unauthorized();
        }

        var result = await mediator.Send(
            new GetTransactionDetailQuery(
                tenantPublicId,
                userPublicId,
                role,
                transactionPublicId),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet]
    public async Task<IActionResult> GetTransactionsAsync(
        [FromQuery] string? type,
        [FromQuery] string? status,
        [FromQuery] string? paymentStatus,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var tenantPublicId = User.FindFirst(JwtClaimNames.Tenant)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(tenantPublicId))
        {
            return Unauthorized();
        }

        var result = await mediator.Send(
            new GetTransactionsQuery(
                tenantPublicId,
                role,
                type,
                status,
                paymentStatus,
                pageNumber,
                pageSize),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetTransactionSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var tenantPublicId = User.FindFirst(JwtClaimNames.Tenant)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(tenantPublicId))
        {
            return Unauthorized();
        }

        var result = await mediator.Send(
            new GetTransactionSummaryQuery(
                tenantPublicId,
                role),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("procurements")]
    public async Task<IActionResult> CreateProcurementAsync(
        [FromBody] CreateProcurementRequest request,
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
            new CreateProcurementCommand(
                tenantPublicId,
                userPublicId,
                request),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPut("procurements/{transactionPublicId}")]
    public async Task<IActionResult> UpdateProcurementAsync(
        [FromRoute] string transactionPublicId,
        [FromBody] UpdateProcurementRequest request,
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
            new UpdateProcurementCommand(
                tenantPublicId,
                userPublicId,
                transactionPublicId,
                request),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("{transactionPublicId}/submit")]
    public async Task<IActionResult> SubmitProcurementAsync(
        [FromRoute] string transactionPublicId,
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
            new SubmitProcurementCommand(
                tenantPublicId,
                userPublicId,
                transactionPublicId),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("{transactionPublicId}/approve")]
    public async Task<IActionResult> ApproveProcurementAsync(
        [FromRoute] string transactionPublicId,
        CancellationToken cancellationToken = default)
    {
        var tenantPublicId = User.FindFirst(JwtClaimNames.Tenant)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(tenantPublicId))
        {
            return Unauthorized();
        }

        var result = await mediator.Send(
            new ApproveProcurementCommand(
                tenantPublicId,
                role,
                transactionPublicId),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("{transactionPublicId}/reject")]
    public async Task<IActionResult> RejectProcurementAsync(
        [FromRoute] string transactionPublicId,
        [FromBody] RejectProcurementRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantPublicId = User.FindFirst(JwtClaimNames.Tenant)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(tenantPublicId))
        {
            return Unauthorized();
        }

        var result = await mediator.Send(
            new RejectProcurementCommand(
                tenantPublicId,
                role,
                transactionPublicId,
                request),
            cancellationToken);

        return result.ToActionResult();
    }
}