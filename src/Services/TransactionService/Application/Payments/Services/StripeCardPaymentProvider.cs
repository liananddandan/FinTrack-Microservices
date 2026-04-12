using SharedKernel.Common.Results;
using SharedKernel.Contracts.Payments;
using TransactionService.Application.Payments.Abstractions;
using TransactionService.Application.Payments.Dtos;

namespace TransactionService.Application.Payments.Services;

public class StripeCardPaymentProvider(IStripePaymentGateway stripePaymentGateway)
    : IPaymentProvider
{
    public string Provider => PaymentProviders.Stripe;
    public string PaymentMethodType => PaymentMethodTypes.Card;

    public async Task<ServiceResult<CreateProviderPaymentResult>> CreatePaymentAsync(
        CreateProviderPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ConnectedAccountId))
        {
            return ServiceResult<CreateProviderPaymentResult>.Fail(
                "STRIPE_CONNECT_NOT_READY",
                "Stripe is not connected for current tenant.");
        }

        var stripeResult = await stripePaymentGateway.CreateCardPaymentIntentAsync(
            request.Amount,
            request.Currency,
            request.ConnectedAccountId,
            request.OrderPublicId,
            request.PaymentPublicId,
            cancellationToken);

        if (!stripeResult.Success || stripeResult.Data is null)
        {
            return ServiceResult<CreateProviderPaymentResult>.Fail(
                stripeResult.Code ?? "STRIPE_PAYMENT_INTENT_CREATE_FAILED",
                stripeResult.Message ?? "Failed to create Stripe payment intent.");
        }

        var result = new CreateProviderPaymentResult(
            ExternalPaymentId: stripeResult.Data.PaymentIntentId,
            ExternalChargeId: stripeResult.Data.ChargeId,
            ClientSecret: stripeResult.Data.ClientSecret,
            InitialStatus: MapStripeStatus(stripeResult.Data.Status),
            FailureReason: null);

        return ServiceResult<CreateProviderPaymentResult>.Ok(
            result,
            "STRIPE_PAYMENT_CREATED",
            "Stripe card payment created successfully.");
    }

    private static string MapStripeStatus(string stripeStatus)
    {
        return stripeStatus switch
        {
            "requires_payment_method" => PaymentStatuses.Pending,
            "requires_confirmation" => PaymentStatuses.Pending,
            "requires_action" => PaymentStatuses.RequiresAction,
            "processing" => PaymentStatuses.Processing,
            "succeeded" => PaymentStatuses.Succeeded,
            "canceled" => PaymentStatuses.Cancelled,
            _ => PaymentStatuses.Pending
        };
    }
}