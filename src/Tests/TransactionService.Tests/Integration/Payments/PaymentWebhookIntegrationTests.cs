namespace TransactionService.Tests.Integration.Payments;

using System.Net;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SharedKernel.Common.Options;
using Xunit;

[Collection("NonParallel Collection")]
public class PaymentWebhookIntegrationTests :
    IClassFixture<TransactionWebApplicationFactory<Program>>
{
    private readonly TransactionWebApplicationFactory<Program> _baseFactory;
    private readonly WebApplicationFactory<Program> _factory;
    private const string WebhookSecret = "whsec_test_secret";

    public PaymentWebhookIntegrationTests(TransactionWebApplicationFactory<Program> factory)
    {
        _baseFactory = factory;

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.Configure<StripeOptions>(options =>
                {
                    options.WebhookSecret = WebhookSecret;
                });
            });
        });   
    }

    [Fact]
    public async Task Webhook_Should_Return_200_When_Request_Is_Signed_Correctly()
    {
        var client = _factory.CreateClient();

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

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/payments/webhook");
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
        request.Headers.Add(
            "Stripe-Signature",
            StripeWebhookTestHelper.CreateSignatureHeader(payload, WebhookSecret));

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Webhook_Should_Return_400_When_Signature_Is_Invalid()
    {
        var client = _factory.CreateClient();

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

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/payments/webhook");
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
        request.Headers.Add("Stripe-Signature", "t=123,v1=invalid");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}