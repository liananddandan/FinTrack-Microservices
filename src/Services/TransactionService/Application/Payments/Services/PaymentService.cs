using SharedKernel.Common.Results;
using SharedKernel.Contracts.Payments;
using TransactionService.Application.Common.Abstractions;
using TransactionService.Application.Orders.Abstractions;
using TransactionService.Application.Payments.Abstractions;
using TransactionService.Application.Payments.Dtos;
using TransactionService.Domain.Entities;

namespace TransactionService.Application.Payments.Services;

public class PaymentService(
    IOrderRepository orderRepository,
    IPaymentRepository paymentRepository,
    IPaymentProviderResolver paymentProviderResolver,
    ITenantPaymentConfigService tenantPaymentConfigService,
    ICurrentTenantContext currentTenantContext,
    IUnitOfWork unitOfWork,
    ILogger<PaymentService> logger)
    : IPaymentService
{
    public async Task<ServiceResult<CreatePaymentResultDto>> CreatePaymentAsync(
        string orderPublicId,
        string paymentMethodType,
        CancellationToken cancellationToken = default)
    {
        var tenantPublicId = currentTenantContext.TenantPublicId;
        if (tenantPublicId == Guid.Empty)
        {
            return ServiceResult<CreatePaymentResultDto>.Fail(
                "TENANT_CONTEXT_MISSING",
                "Tenant context is missing.");
        }
        
        if (!Guid.TryParse(orderPublicId, out var parsedOrderPublicId))
        {
            return ServiceResult<CreatePaymentResultDto>.Fail(
                "ORDER_PUBLIC_ID_INVALID",
                "Order id is invalid.");
        }

        if (string.IsNullOrWhiteSpace(paymentMethodType))
        {
            return ServiceResult<CreatePaymentResultDto>.Fail(
                "PAYMENT_METHOD_TYPE_REQUIRED",
                "Payment method type is required.");
        }

        var order = await orderRepository.GetByPublicIdAsync(
            tenantPublicId,
            parsedOrderPublicId,
            cancellationToken);

        if (order is null || order.IsDeleted)
        {
            return ServiceResult<CreatePaymentResultDto>.Fail(
                "ORDER_NOT_FOUND",
                "Order not found.");
        }

        if (order.TenantPublicId != tenantPublicId)
        {
            return ServiceResult<CreatePaymentResultDto>.Fail(
                "ORDER_TENANT_MISMATCH",
                "Order does not belong to current tenant.");
        }

        if (order.PaymentStatus == PaymentStatuses.Succeeded)
        {
            return ServiceResult<CreatePaymentResultDto>.Fail(
                "ORDER_ALREADY_PAID",
                "Order has already been paid.");
        }

        var hasSucceededPayment = await paymentRepository.ExistsSucceededPaymentByOrderIdAsync(
            order.Id,
            cancellationToken);

        if (hasSucceededPayment)
        {
            return ServiceResult<CreatePaymentResultDto>.Fail(
                "ORDER_ALREADY_PAID",
                "Order has already been paid.");
        }

        var provider = ResolveProvider(paymentMethodType);
        if (string.IsNullOrWhiteSpace(provider))
        {
            return ServiceResult<CreatePaymentResultDto>.Fail(
                "PAYMENT_METHOD_NOT_SUPPORTED",
                "Payment method is not supported.");
        }

        string? connectedAccountId = null;

        if (provider == PaymentProviders.Stripe)
        {
            connectedAccountId = await tenantPaymentConfigService.GetStripeConnectedAccountIdAsync(
                tenantPublicId,
                cancellationToken);

            if (string.IsNullOrWhiteSpace(connectedAccountId))
            {
                return ServiceResult<CreatePaymentResultDto>.Fail(
                    "STRIPE_CONNECT_NOT_READY",
                    "Stripe is not connected for current tenant.");
            }
        }

        var payment = new Payment
        {
            TenantPublicId = tenantPublicId,
            OrderId = order.Id,
            OrderPublicId = order.PublicId,
            Provider = provider,
            PaymentMethodType = paymentMethodType,
            Currency = "NZD",
            Amount = order.TotalAmount,
            RefundedAmount = 0,
            Status = PaymentStatuses.Pending,
            StripeConnectedAccountId = connectedAccountId
        };

        await paymentRepository.AddAsync(payment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var paymentProvider = paymentProviderResolver.Resolve(provider, paymentMethodType);

        var providerResult = await paymentProvider.CreatePaymentAsync(
            new CreateProviderPaymentRequest(
                PaymentPublicId: payment.PublicId.ToString(),
                OrderPublicId: payment.OrderPublicId.ToString(),
                TenantPublicId: tenantPublicId,
                Amount: payment.Amount,
                Currency: payment.Currency,
                ConnectedAccountId: connectedAccountId),
            cancellationToken);

        if (!providerResult.Success || providerResult.Data is null)
        {
            payment.Status = PaymentStatuses.Failed;
            payment.FailedAt = DateTime.UtcNow;
            payment.FailureReason = providerResult.Message;
            payment.UpdatedAt = DateTime.UtcNow;

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult<CreatePaymentResultDto>.Fail(
                providerResult.Code ?? "PAYMENT_PROVIDER_CREATE_FAILED",
                providerResult.Message ?? "Failed to create payment.");
        }

        payment.ProviderPaymentIntentId = providerResult.Data.ExternalPaymentId;
        payment.ProviderChargeId = providerResult.Data.ExternalChargeId;
        payment.Status = providerResult.Data.InitialStatus;
        payment.UpdatedAt = DateTime.UtcNow;

        if (provider == PaymentProviders.Cash &&
            payment.Status == PaymentStatuses.Succeeded)
        {
            payment.PaidAt = DateTime.UtcNow;

            order.PaymentStatus = PaymentStatuses.Succeeded;
            order.PaymentMethod = paymentMethodType;
            order.PaidAt = payment.PaidAt;
        }
        else
        {
            order.PaymentStatus = payment.Status;
            order.PaymentMethod = paymentMethodType;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<CreatePaymentResultDto>.Ok(
            new CreatePaymentResultDto(
                PaymentPublicId: payment.PublicId.ToString(),
                Provider: payment.Provider,
                PaymentMethodType: payment.PaymentMethodType,
                Status: payment.Status,
                ClientSecret: providerResult.Data.ClientSecret,
                StripeConnectedAccountId: payment.StripeConnectedAccountId),
            "PAYMENT_CREATED",
            "Payment created successfully.");
    }

    public async Task<ServiceResult<PaymentDetailDto>> GetPaymentByPublicIdAsync(
        string paymentPublicId,
        CancellationToken cancellationToken = default)
    {
        var tenantPublicId = currentTenantContext.TenantPublicId;
        if (tenantPublicId == Guid.Empty)
        {
            return ServiceResult<PaymentDetailDto>.Fail(
                "TENANT_CONTEXT_MISSING",
                "Tenant context is missing.");
        }
        
        if (!Guid.TryParse(paymentPublicId, out var parsedPaymentPublicId))
        {
            return ServiceResult<PaymentDetailDto>.Fail(
                "PAYMENT_PUBLIC_ID_INVALID",
                "Payment id is invalid.");
        }

        var payment = await paymentRepository.GetByPublicIdAsync(
            parsedPaymentPublicId,
            cancellationToken);

        if (payment is null || payment.IsDeleted)
        {
            return ServiceResult<PaymentDetailDto>.Fail(
                "PAYMENT_NOT_FOUND",
                "Payment not found.");
        }

        if (payment.TenantPublicId != tenantPublicId)
        {
            return ServiceResult<PaymentDetailDto>.Fail(
                "PAYMENT_TENANT_MISMATCH",
                "Payment does not belong to current tenant.");
        }

        return ServiceResult<PaymentDetailDto>.Ok(
            new PaymentDetailDto(
                PaymentPublicId: payment.PublicId.ToString(),
                OrderPublicId: payment.OrderPublicId.ToString(),
                Provider: payment.Provider,
                PaymentMethodType: payment.PaymentMethodType,
                Status: payment.Status,
                Currency: payment.Currency,
                Amount: payment.Amount,
                RefundedAmount: payment.RefundedAmount,
                FailureReason: payment.FailureReason,
                CreatedAt: payment.CreatedAt,
                PaidAt: payment.PaidAt,
                FailedAt: payment.FailedAt,
                RefundedAt: payment.RefundedAt),
            "PAYMENT_DETAIL_OK",
            "Payment detail loaded.");
    }

    public async Task<ServiceResult<List<PaymentListItemDto>>> GetPaymentsByOrderPublicIdAsync(
        string orderPublicId,
        CancellationToken cancellationToken = default)
    {
        var tenantPublicId = currentTenantContext.TenantPublicId;
        if (tenantPublicId == Guid.Empty)
        {
            return ServiceResult<List<PaymentListItemDto>>.Fail(
                "TENANT_CONTEXT_MISSING",
                "Tenant context is missing.");
        }
        if (!Guid.TryParse(orderPublicId, out var parsedOrderPublicId))
        {
            return ServiceResult<List<PaymentListItemDto>>.Fail(
                "ORDER_PUBLIC_ID_INVALID",
                "Order id is invalid.");
        }

        var order = await orderRepository.GetByPublicIdAsync(
            tenantPublicId,
            parsedOrderPublicId,
            cancellationToken);

        if (order is null || order.IsDeleted)
        {
            return ServiceResult<List<PaymentListItemDto>>.Fail(
                "ORDER_NOT_FOUND",
                "Order not found.");
        }

        if (order.TenantPublicId != tenantPublicId)
        {
            return ServiceResult<List<PaymentListItemDto>>.Fail(
                "ORDER_TENANT_MISMATCH",
                "Order does not belong to current tenant.");
        }

        var payments = await paymentRepository.GetByOrderIdAsync(
            order.Id,
            cancellationToken);

        var items = payments
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new PaymentListItemDto(
                PaymentPublicId: x.PublicId.ToString(),
                Provider: x.Provider,
                PaymentMethodType: x.PaymentMethodType,
                Status: x.Status,
                Currency: x.Currency,
                Amount: x.Amount,
                CreatedAt: x.CreatedAt,
                PaidAt: x.PaidAt))
            .ToList();

        return ServiceResult<List<PaymentListItemDto>>.Ok(
            items,
            "ORDER_PAYMENTS_OK",
            "Order payments loaded.");
    }

    private static string ResolveProvider(string paymentMethodType)
    {
        return paymentMethodType switch
        {
            PaymentMethodTypes.Card => PaymentProviders.Stripe,
            PaymentMethodTypes.Terminal => PaymentProviders.Stripe,
            PaymentMethodTypes.Cash => PaymentProviders.Cash,
            _ => string.Empty
        };
    }
}