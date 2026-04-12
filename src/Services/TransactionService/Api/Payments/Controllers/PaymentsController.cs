using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Common.DTOs.Auth;
using TransactionService.Api.Payments.Contracts;
using TransactionService.Application.Payments.Commands;
using TransactionService.Application.Payments.Queries;

namespace TransactionService.Api.Payments.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreatePayment(
        [FromBody] CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreatePaymentCommand(
                request.OrderPublicId,
                request.PaymentMethodType),
            cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{paymentPublicId}")]
    public async Task<IActionResult> GetPaymentByPublicId(
        string paymentPublicId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetPaymentByPublicIdQuery(paymentPublicId),
            cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }
}