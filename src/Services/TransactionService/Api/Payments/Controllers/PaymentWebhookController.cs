using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Options;
using Stripe;
using TransactionService.Application.Common.Extensions;
using TransactionService.Application.Payments.Commands;

namespace TransactionService.Api.Payments.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentWebhookController(
    IMediator mediator,
    IOptions<StripeOptions> stripeOptions) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> HandleStripeWebhook(
        CancellationToken cancellationToken)
    {
        var json = await new StreamReader(Request.Body).ReadToEndAsync(cancellationToken);

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                stripeOptions.Value.WebhookSecret,
                throwOnApiVersionMismatch: false);

            var result = await mediator.Send(
                new HandleStripeWebhookCommand(stripeEvent),
                cancellationToken);

            return result.ToActionResult();
        }
        catch (StripeException ex)
        {
            Console.WriteLine("Stripe webhook signature validation failed:");
            Console.WriteLine(ex.Message);

            return BadRequest(new ApiResponse<object>(
                "Payment.WebhookInvalid",
                ex.Message,
                null));
        }
    }
}