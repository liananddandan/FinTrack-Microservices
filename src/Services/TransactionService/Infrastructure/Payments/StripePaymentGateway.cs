using Microsoft.Extensions.Options;
using SharedKernel.Common.Options;
using Stripe;
using TransactionService.Application.Payments.Abstractions;
using TransactionService.Application.Payments.Dtos;
using TransactionService.Domain.Constants;

namespace TransactionService.Infrastructure.Payments;

public class StripePaymentGateway(IOptions<StripeOptions> stripeOptions) : IPaymentGateway
{
    public string Provider => PaymentProviders.Stripe;

    public async Task<CreatePaymentGatewayResult> CreatePaymentAsync(
        CreatePaymentGatewayRequest request,
        CancellationToken cancellationToken = default)
    {
        StripeConfiguration.ApiKey = stripeOptions.Value.SecretKey;

        var service = new PaymentIntentService();

        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(request.Amount * 100m),
            Currency = request.Currency.ToLowerInvariant(),
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true
            },
            Description = request.Description,
            Metadata = new Dictionary<string, string>
            {
                ["orderPublicId"] = request.OrderPublicId.ToString(),
                ["orderNumber"] = request.OrderNumber
            }
        };

        var requestOptions = new RequestOptions
        {
            IdempotencyKey = request.IdempotencyKey
        };

        var paymentIntent = await service.CreateAsync(
            options,
            requestOptions,
            cancellationToken);

        return new CreatePaymentGatewayResult
        {
            ProviderPaymentReference = paymentIntent.Id,
            ClientSecret = paymentIntent.ClientSecret,
            Status = paymentIntent.Status
        };
    }
}