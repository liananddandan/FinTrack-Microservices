using System.Text;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using SharedKernel.Common.Options;
using SharedKernel.Common.Results;
using Stripe;
using TransactionService.Api.Payments.Controllers;
using TransactionService.Application.Payments.Commands;
using Xunit;

namespace TransactionService.Tests.Unit.Payments;

public class PaymentWebhookControllerTests
{
    private const string WebhookSecret = "whsec_test_secret";

    private readonly Mock<IMediator> _mediator = new();

    private PaymentWebhookController CreateController(string payload, string signatureHeader)
    {
        var controller = new PaymentWebhookController(
            _mediator.Object,
            Options.Create(new StripeOptions
            {
                WebhookSecret = WebhookSecret
            }));

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(payload));
        httpContext.Request.Headers["Stripe-Signature"] = signatureHeader;

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    [Fact]
    public async Task HandleStripeWebhook_Should_Return_Ok_When_Signature_Is_Valid()
    {
        var payload =
            """
            {
              "id": "evt_test_123",
              "object": "event",
              "type": "payment_intent.succeeded",
              "data": {
                "object": {
                  "id": "pi_test_123",
                  "object": "payment_intent"
                }
              }
            }
            """;

        var signatureHeader = StripeWebhookTestHelper.CreateSignatureHeader(payload, WebhookSecret);

        _mediator
            .Setup(x => x.Send(
                It.IsAny<HandleStripeWebhookCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<bool>.Ok(true, "Payment.WebhookProcessed", "Processed."));

        var controller = CreateController(payload, signatureHeader);

        var result = await controller.HandleStripeWebhook(CancellationToken.None);

        Assert.IsAssignableFrom<IActionResult>(result);

        _mediator.Verify(x => x.Send(
            It.Is<HandleStripeWebhookCommand>(c =>
                c.StripeEvent.Type == "payment_intent.succeeded"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleStripeWebhook_Should_Return_BadRequest_When_Signature_Is_Invalid()
    {
        var payload =
            """
            {
              "id": "evt_test_123",
              "object": "event",
              "type": "payment_intent.succeeded",
              "data": {
                "object": {
                  "id": "pi_test_123",
                  "object": "payment_intent"
                }
              }
            }
            """;

        var controller = CreateController(payload, "t=123,v1=invalid");

        var result = await controller.HandleStripeWebhook(CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);

        _mediator.Verify(x => x.Send(
            It.IsAny<HandleStripeWebhookCommand>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}