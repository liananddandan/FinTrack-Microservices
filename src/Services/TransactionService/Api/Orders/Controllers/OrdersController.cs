using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Api.Orders.Contracts;
using TransactionService.Application.Common.Extensions;
using TransactionService.Application.Orders.Commands;
using TransactionService.Application.Orders.Queries;

namespace TransactionService.Api.Orders.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand(
            request.CustomerName,
            request.CustomerPhone,
            request.PaymentMethod,
            request.Items.Select(x => new CreateOrderItemCommand(
                x.ProductPublicId,
                x.Quantity,
                x.Notes)).ToList());

        var result = await mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{orderPublicId:guid}")]
    public async Task<IActionResult> GetById(
        Guid orderPublicId,
        CancellationToken cancellationToken)
    {
        var query = new GetOrderByPublicIdQuery(orderPublicId);
        var result = await mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] bool createdByMe = false,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtc = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetOrdersQuery(
            createdByMe,
            status,
            fromUtc,
            toUtc,
            pageNumber,
            pageSize);

        var result = await mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("{orderPublicId:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        Guid orderPublicId,
        CancellationToken cancellationToken)
    {
        var command = new CancelOrderCommand(orderPublicId);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }
    
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        [FromQuery] bool createdByMe = false,
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtc = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetOrderSummaryQuery(
            createdByMe,
            fromUtc,
            toUtc);

        var result = await mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }
}