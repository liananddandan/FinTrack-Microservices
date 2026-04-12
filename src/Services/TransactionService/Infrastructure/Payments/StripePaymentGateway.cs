using Microsoft.Extensions.Options;
using SharedKernel.Common.Results;
using Stripe;
using TransactionService.Application.Payments.Abstractions;
using TransactionService.Application.Payments.Dtos;
using TransactionService.Application.Payments.Options;

namespace TransactionService.Infrastructure.Payments;

public class StripePaymentGateway(
    IOptions<StripePaymentOptions> options,
    ILogger<StripePaymentGateway> logger)
    : IStripePaymentGateway
{
    private readonly StripePaymentOptions _options = options.Value;

    public async Task<ServiceResult<CreateStripePaymentIntentResult>> CreateCardPaymentIntentAsync(
        decimal amount,
        string currency,
        string connectedAccountId,
        string orderPublicId,
        string paymentPublicId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            logger.LogError("Stripe SecretKey is not configured.");

            return ServiceResult<CreateStripePaymentIntentResult>.Fail(
                "STRIPE_SECRET_KEY_MISSING",
                "Stripe secret key is not configured.");
        }

        if (string.IsNullOrWhiteSpace(connectedAccountId))
        {
            return ServiceResult<CreateStripePaymentIntentResult>.Fail(
                "STRIPE_CONNECTED_ACCOUNT_ID_REQUIRED",
                "Stripe connected account id is required.");
        }

        try
        {
            StripeConfiguration.ApiKey = _options.SecretKey;

            var service = new PaymentIntentService();

            var requestOptions = new RequestOptions
            {
                StripeAccount = connectedAccountId
            };

            var createOptions = new PaymentIntentCreateOptions
            {
                Amount = ToStripeAmount(amount, currency),
                Currency = currency.ToLowerInvariant(),

                PaymentMethodTypes = ["card"],

                AutomaticPaymentMethods = null,

                Metadata = new Dictionary<string, string>
                {
                    ["orderPublicId"] = orderPublicId,
                    ["paymentPublicId"] = paymentPublicId,
                    ["connectedAccountId"] = connectedAccountId
                }
            };

            var paymentIntent = await service.CreateAsync(
                createOptions,
                requestOptions,
                cancellationToken);

            if (string.IsNullOrWhiteSpace(paymentIntent.ClientSecret))
            {
                logger.LogError(
                    "Stripe PaymentIntent created without client secret. PaymentIntentId: {PaymentIntentId}",
                    paymentIntent.Id);

                return ServiceResult<CreateStripePaymentIntentResult>.Fail(
                    "STRIPE_PAYMENT_INTENT_CLIENT_SECRET_MISSING",
                    "Stripe payment intent client secret is missing.");
            }

            var result = new CreateStripePaymentIntentResult(
                PaymentIntentId: paymentIntent.Id,
                ChargeId: null,
                ClientSecret: paymentIntent.ClientSecret,
                Status: paymentIntent.Status);

            return ServiceResult<CreateStripePaymentIntentResult>.Ok(
                result,
                "STRIPE_PAYMENT_INTENT_CREATED",
                "Stripe payment intent created successfully.");
        }
        catch (StripeException ex)
        {
            logger.LogError(
                ex,
                "Stripe error when creating payment intent. ConnectedAccountId: {ConnectedAccountId}, OrderPublicId: {OrderPublicId}, PaymentPublicId: {PaymentPublicId}",
                connectedAccountId,
                orderPublicId,
                paymentPublicId);

            return ServiceResult<CreateStripePaymentIntentResult>.Fail(
                "STRIPE_PAYMENT_INTENT_CREATE_FAILED",
                ex.StripeError?.Message ?? ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unexpected error when creating Stripe payment intent. ConnectedAccountId: {ConnectedAccountId}, OrderPublicId: {OrderPublicId}, PaymentPublicId: {PaymentPublicId}",
                connectedAccountId,
                orderPublicId,
                paymentPublicId);

            return ServiceResult<CreateStripePaymentIntentResult>.Fail(
                "STRIPE_PAYMENT_INTENT_CREATE_FAILED",
                "Failed to create Stripe payment intent.");
        }
    }

    private static long ToStripeAmount(decimal amount, string currency)
    {
        return (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);
    }
}