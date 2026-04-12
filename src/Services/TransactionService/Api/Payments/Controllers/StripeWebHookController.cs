using Microsoft.AspNetCore.Mvc;
using TransactionService.Application.Payments.Abstractions;

namespace TransactionService.Api.Payments.Controllers;

[ApiController]
[Route("api/payments/stripe/webhook")]
public class StripeWebhookController(IStripeWebhookService stripeWebhookService,
    ILogger<StripeWebhookController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Handle(CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Stripe webhook received. Headers: {Headers}",
            string.Join(", ",
                Request.Headers.Select(x => $"{x.Key}={x.Value}")));
        using var reader = new StreamReader(Request.Body);
        var json = await reader.ReadToEndAsync(cancellationToken);

        var signatureHeader = Request.Headers["Stripe-Signature"].ToString();
        if (string.IsNullOrWhiteSpace(signatureHeader))
        {
            return BadRequest("Missing Stripe-Signature header.");
        }

        var result = await stripeWebhookService.HandleWebhookAsync(
            json,
            signatureHeader,
            cancellationToken);

        return result.Success ? Ok() : BadRequest(result);
    }
}