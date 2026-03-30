using SharedKernel.Common.Results;
using TransactionService.Application.Common.Abstractions;
using TransactionService.Application.Orders.Abstractions;
using TransactionService.Application.Payments.Abstractions;
using TransactionService.Application.Payments.Commands;
using TransactionService.Application.Payments.Dtos;
using TransactionService.Application.Payments.Queries;
using TransactionService.Domain.Constants;
using TransactionService.Domain.Entities;

namespace TransactionService.Application.Payments.Services;

public class PaymentService(
    ICurrentTenantContext currentTenantContext,
    IOrderRepository orderRepository,
    IPaymentRepository paymentRepository,
    IPaymentGatewayResolver paymentGatewayResolver,
    IUnitOfWork unitOfWork) : IPaymentService
{
    public async Task<ServiceResult<PaymentDto>> CreateAsync(
        CreatePaymentCommand command,
        CancellationToken cancellationToken = default)
    {
        if (currentTenantContext.TenantPublicId == Guid.Empty)
        {
            return ServiceResult<PaymentDto>.Fail(
                ResultCodes.Forbidden,
                "Tenant context is missing.");
        }

        var order = await orderRepository.GetByPublicIdAsync(
            currentTenantContext.TenantPublicId,
            command.OrderPublicId,
            cancellationToken);

        if (order is null)
        {
            return ServiceResult<PaymentDto>.Fail(
                ResultCodes.Order.NotFound,
                "Order not found.");
        }

        if (order.PaymentStatus == PaymentStatuses.Paid)
        {
            return ServiceResult<PaymentDto>.Fail(
                ResultCodes.Order.InvalidStatus,
                "Order is already paid.");
        }

        var payment = await paymentRepository.GetByOrderIdAsync(
            order.Id,
            cancellationToken);

        if (payment is not null && payment.Status == PaymentStatuses.Pending)
        {
            return ServiceResult<PaymentDto>.Ok(
                MapToDto(payment, order.PublicId),
                ResultCodes.Payment.CreateSuccess,
                "Existing pending payment returned.");
        }

        if (payment is null)
        {
            payment = new Payment
            {
                TenantPublicId = order.TenantPublicId,
                OrderId = order.Id,
                Provider = command.Provider,
                PaymentMethod = command.PaymentMethod,
                Amount = order.TotalAmount,
                Currency = "NZD",
                IdempotencyKey = $"payment-{order.PublicId:N}",
                CreatedAt = DateTime.UtcNow
            };

            await paymentRepository.AddAsync(payment, cancellationToken);
        }
        else
        {
            payment.Provider = command.Provider;
            payment.PaymentMethod = command.PaymentMethod;
            payment.Amount = order.TotalAmount;
            payment.Currency = "NZD";
            payment.IdempotencyKey = $"payment-{order.PublicId:N}";
            payment.FailureReason = null;
        }

        var gateway = paymentGatewayResolver.Resolve(command.Provider);

        var gatewayResult = await gateway.CreatePaymentAsync(
            new CreatePaymentGatewayRequest
            {
                IdempotencyKey = payment.IdempotencyKey,
                OrderPublicId = order.PublicId,
                OrderNumber = order.OrderNumber,
                Amount = order.TotalAmount,
                Currency = payment.Currency,
                PaymentMethod = command.PaymentMethod,
                Description = $"Order {order.OrderNumber}"
            },
            cancellationToken);

        payment.Status = PaymentStatuses.Pending;
        payment.ProviderPaymentReference = gatewayResult.ProviderPaymentReference;
        payment.ProviderClientSecret = gatewayResult.ClientSecret;
        payment.StartedAt = DateTime.UtcNow;

        order.PaymentMethod = command.PaymentMethod;
        order.PaymentStatus = PaymentStatuses.Pending;
        order.Status = OrderStatuses.Pending;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<PaymentDto>.Ok(
            MapToDto(payment, order.PublicId),
            ResultCodes.Payment.CreateSuccess,
            "Payment created successfully.");
    }

    public async Task<ServiceResult<PaymentDto>> GetByOrderAsync(
        GetPaymentByOrderQuery query,
        CancellationToken cancellationToken = default)
    {
        if (currentTenantContext.TenantPublicId == Guid.Empty)
        {
            return ServiceResult<PaymentDto>.Fail(
                ResultCodes.Forbidden,
                "Tenant context is missing.");
        }

        var payment = await paymentRepository.GetByOrderPublicIdAsync(
            currentTenantContext.TenantPublicId,
            query.OrderPublicId,
            cancellationToken);

        if (payment is null)
        {
            return ServiceResult<PaymentDto>.Fail(
                ResultCodes.Payment.NotFound,
                "Payment not found.");
        }

        return ServiceResult<PaymentDto>.Ok(
            MapToDto(payment, query.OrderPublicId),
            ResultCodes.Payment.GetSuccess,
            "Payment retrieved successfully.");
    }

    private static PaymentDto MapToDto(Payment payment, Guid orderPublicId)
    {
        return new PaymentDto
        {
            PaymentPublicId = payment.PublicId,
            OrderPublicId = orderPublicId,
            Provider = payment.Provider,
            PaymentMethod = payment.PaymentMethod,
            Status = payment.Status,
            Amount = payment.Amount,
            Currency = payment.Currency,
            ProviderPaymentReference = payment.ProviderPaymentReference,
            ProviderClientSecret = payment.ProviderClientSecret,
            FailureReason = payment.FailureReason,
            CreatedAt = payment.CreatedAt,
            StartedAt = payment.StartedAt,
            PaidAt = payment.PaidAt,
            RefundedAt = payment.RefundedAt
        };
    }
}