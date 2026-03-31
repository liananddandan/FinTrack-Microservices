using MediatR;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Api.Payments.Contracts;
using TransactionService.Application.Common.Extensions;
using TransactionService.Application.Payments.Commands;
using TransactionService.Application.Payments.Queries;

namespace TransactionService.Api.Payments.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentController(
    IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreatePaymentCommand(
                request.OrderPublicId,
                request.Provider,
                request.PaymentMethod),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("by-order/{orderPublicId:guid}")]
    public async Task<IActionResult> GetByOrderAsync(
        Guid orderPublicId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetPaymentByOrderQuery(orderPublicId),
            cancellationToken);

        return result.ToActionResult();
    }
}