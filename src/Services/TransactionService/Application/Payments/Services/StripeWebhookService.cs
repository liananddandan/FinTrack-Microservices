using Microsoft.Extensions.Options;
using SharedKernel.Common.Results;
using SharedKernel.Contracts.Payments;
using Stripe;
using TransactionService.Application.Common.Abstractions;
using TransactionService.Application.Common.Options;
using TransactionService.Application.Orders.Abstractions;
using TransactionService.Application.Payments.Abstractions;
using TransactionService.Domain.Entities;

namespace TransactionService.Application.Payments.Services;


public class StripeWebhookService(
    IOptions<StripeWebhookOptions> webhookOptions,
    IPaymentRepository paymentRepository,
    IPaymentWebhookEventRepository paymentWebhookEventRepository,
    IOrderRepository orderRepository,
    ICurrentTenantContext currentTenantContext,
    IUnitOfWork unitOfWork,
    ILogger<StripeWebhookService> logger)
    : IStripeWebhookService
{
    private readonly StripeWebhookOptions _webhookOptions = webhookOptions.Value;

    public async Task<ServiceResult<bool>> HandleWebhookAsync(
        string json,
        string signatureHeader,
        CancellationToken cancellationToken = default)
    {
        Event stripeEvent;

        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                json,
                signatureHeader,
                _webhookOptions.Secret,
                throwOnApiVersionMismatch:false);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to verify Stripe webhook signature.");

            return ServiceResult<bool>.Fail(
                "STRIPE_WEBHOOK_SIGNATURE_INVALID",
                "Stripe webhook signature is invalid.");
        }

        var alreadyHandled = await paymentWebhookEventRepository.ExistsAsync(
            PaymentProviders.Stripe,
            stripeEvent.Id,
            cancellationToken);

        if (alreadyHandled)
        {
            return ServiceResult<bool>.Ok(
                true,
                "STRIPE_WEBHOOK_DUPLICATED",
                "Stripe webhook event already handled.");
        }

        switch (stripeEvent.Type)
        {
            case "payment_intent.succeeded":
                await HandlePaymentIntentSucceededAsync(stripeEvent, cancellationToken);
                break;

            case "payment_intent.payment_failed":
                await HandlePaymentIntentFailedAsync(stripeEvent, cancellationToken);
                break;

            case "payment_intent.canceled":
                await HandlePaymentIntentCanceledAsync(stripeEvent, cancellationToken);
                break;

            default:
                break;
        }

        await paymentWebhookEventRepository.AddAsync(
            new PaymentWebhookEvent
            {
                Provider = PaymentProviders.Stripe,
                EventId = stripeEvent.Id,
                EventType = stripeEvent.Type,
                ReceivedAt = DateTime.UtcNow
            },
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<bool>.Ok(
            true,
            "STRIPE_WEBHOOK_HANDLED",
            "Stripe webhook handled successfully.");
    }

    private async Task HandlePaymentIntentSucceededAsync(
        Event stripeEvent,
        CancellationToken cancellationToken)
    {
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
        if (paymentIntent == null)
        {
            return;
        }

        var payment = await FindPaymentByPaymentIntentIdAsync(paymentIntent.Id, cancellationToken);
        if (payment == null || payment.IsDeleted)
        {
            return;
        }

        if (payment.Status == PaymentStatuses.Succeeded)
        {
            return;
        }

        payment.Status = PaymentStatuses.Succeeded;
        payment.PaidAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;
        payment.ProviderChargeId = paymentIntent.LatestChargeId;

        var order = await orderRepository.GetByPublicIdAsync(
            payment.TenantPublicId,
            payment.OrderPublicId, 
            cancellationToken);
        if (order != null && !order.IsDeleted && order.PaymentStatus != PaymentStatuses.Succeeded)
        {
            order.PaymentStatus = PaymentStatuses.Succeeded;
            order.PaymentMethod = payment.PaymentMethodType;
            order.PaidAt = payment.PaidAt;
        }
    }

    private async Task HandlePaymentIntentFailedAsync(
        Event stripeEvent,
        CancellationToken cancellationToken)
    {
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
        if (paymentIntent == null)
        {
            return;
        }

        var payment = await FindPaymentByPaymentIntentIdAsync(paymentIntent.Id, cancellationToken);
        if (payment == null || payment.IsDeleted)
        {
            return;
        }

        if (payment.Status == PaymentStatuses.Succeeded)
        {
            return;
        }

        payment.Status = PaymentStatuses.Failed;
        payment.FailedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;
        payment.FailureReason = paymentIntent.LastPaymentError?.Message;

        var order = await orderRepository.GetByPublicIdAsync(
            payment.TenantPublicId,
            payment.OrderPublicId, 
            cancellationToken);
        
        if (order != null && !order.IsDeleted && order.PaymentStatus != PaymentStatuses.Succeeded)
        {
            order.PaymentStatus = PaymentStatuses.Failed;
        }
    }

    private async Task HandlePaymentIntentCanceledAsync(
        Event stripeEvent,
        CancellationToken cancellationToken)
    {
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
        if (paymentIntent == null)
        {
            return;
        }

        var payment = await FindPaymentByPaymentIntentIdAsync(paymentIntent.Id, cancellationToken);
        if (payment == null || payment.IsDeleted)
        {
            return;
        }

        if (payment.Status == PaymentStatuses.Succeeded)
        {
            return;
        }

        payment.Status = PaymentStatuses.Cancelled;
        payment.UpdatedAt = DateTime.UtcNow;

        var order = await orderRepository.GetByPublicIdAsync(
            payment.TenantPublicId,
            payment.OrderPublicId, 
            cancellationToken);
        if (order != null && !order.IsDeleted && order.PaymentStatus != PaymentStatuses.Succeeded)
        {
            order.PaymentStatus = PaymentStatuses.NotStarted;
        }
    }

    private async Task<Payment?> FindPaymentByPaymentIntentIdAsync(
        string paymentIntentId,
        CancellationToken cancellationToken)
    {
        return await paymentRepository.GetByProviderPaymentIntentIdAsync(
            paymentIntentId,
            cancellationToken);
    }
}