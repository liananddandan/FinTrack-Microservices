using Moq;
using Stripe;
using TransactionService.Application.Common.Abstractions;
using TransactionService.Application.Orders.Abstractions;
using TransactionService.Application.Payments.Abstractions;
using TransactionService.Application.Payments.Commands;
using TransactionService.Application.Payments.Services;
using TransactionService.Domain.Constants;
using TransactionService.Domain.Entities;
using Xunit;
using Order = TransactionService.Domain.Entities.Order;

namespace TransactionService.Tests.Unit.Payments;

public class PaymentServiceWebhookTests
{
    private readonly Mock<ICurrentTenantContext> _tenantContext = new();
    private readonly Mock<IOrderRepository> _orderRepository = new();
    private readonly Mock<IPaymentRepository> _paymentRepository = new();
    private readonly Mock<IPaymentGatewayResolver> _paymentGatewayResolver = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly PaymentService _service;

    public PaymentServiceWebhookTests()
    {
        _service = new PaymentService(
            _tenantContext.Object,
            _orderRepository.Object,
            _paymentRepository.Object,
            _paymentGatewayResolver.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleStripeWebhookAsync_Should_Mark_Payment_And_Order_As_Paid_When_Succeeded()
    {
        var order = new Order
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            OrderNumber = "ORD-1001",
            PaymentStatus = PaymentStatuses.Pending,
            Status = OrderStatuses.Pending
        };

        var payment = new Payment
        {
            Id = 1,
            ProviderPaymentReference = "pi_test_123",
            Status = PaymentStatuses.Pending,
            Order = order
        };

        var stripeEvent = new Event
        {
            Type = "payment_intent.succeeded",
            Data = new EventData
            {
                Object = new PaymentIntent
                {
                    Id = "pi_test_123",
                    Object = "payment_intent"
                }
            }
        };

        _paymentRepository
            .Setup(x => x.GetByProviderReferenceAsync("pi_test_123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var result = await _service.HandleStripeWebhookAsync(
            new HandleStripeWebhookCommand(stripeEvent),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.True(result.Data);
        Assert.Equal(PaymentStatuses.Paid, payment.Status);
        Assert.NotNull(payment.PaidAt);
        Assert.Null(payment.FailureReason);

        Assert.Equal(PaymentStatuses.Paid, order.PaymentStatus);
        Assert.Equal(OrderStatuses.Completed, order.Status);
        Assert.NotNull(order.PaidAt);

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleStripeWebhookAsync_Should_Not_Save_When_Payment_Already_Paid()
    {
        var order = new Order
        {
            Id = 1,
            PaymentStatus = PaymentStatuses.Paid,
            Status = OrderStatuses.Completed,
            PaidAt = DateTime.UtcNow
        };

        var payment = new Payment
        {
            Id = 1,
            ProviderPaymentReference = "pi_test_123",
            Status = PaymentStatuses.Paid,
            Order = order,
            PaidAt = DateTime.UtcNow
        };

        var stripeEvent = new Event
        {
            Type = "payment_intent.succeeded",
            Data = new EventData
            {
                Object = new PaymentIntent
                {
                    Id = "pi_test_123",
                    Object = "payment_intent"
                }
            }
        };

        _paymentRepository
            .Setup(x => x.GetByProviderReferenceAsync("pi_test_123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var result = await _service.HandleStripeWebhookAsync(
            new HandleStripeWebhookCommand(stripeEvent),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.True(result.Data);

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleStripeWebhookAsync_Should_Ignore_When_Payment_Not_Found()
    {
        var stripeEvent = new Event
        {
            Type = "payment_intent.succeeded",
            Data = new EventData
            {
                Object = new PaymentIntent
                {
                    Id = "pi_missing",
                    Object = "payment_intent"
                }
            }
        };

        _paymentRepository
            .Setup(x => x.GetByProviderReferenceAsync("pi_missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var result = await _service.HandleStripeWebhookAsync(
            new HandleStripeWebhookCommand(stripeEvent),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.True(result.Data);

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleStripeWebhookAsync_Should_Mark_Payment_And_Order_As_Failed_When_PaymentFailed()
    {
        var order = new Order
        {
            Id = 1,
            PaymentStatus = PaymentStatuses.Pending,
            Status = OrderStatuses.Pending
        };

        var payment = new Payment
        {
            Id = 1,
            ProviderPaymentReference = "pi_test_456",
            Status = PaymentStatuses.Pending,
            Order = order
        };

        var stripeEvent = new Event
        {
            Type = "payment_intent.payment_failed",
            Data = new EventData
            {
                Object = new PaymentIntent
                {
                    Id = "pi_test_456",
                    Object = "payment_intent",
                    LastPaymentError = new StripeError
                    {
                        Message = "Card was declined."
                    }
                }
            }
        };

        _paymentRepository
            .Setup(x => x.GetByProviderReferenceAsync("pi_test_456", It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var result = await _service.HandleStripeWebhookAsync(
            new HandleStripeWebhookCommand(stripeEvent),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.True(result.Data);
        Assert.Equal(PaymentStatuses.Failed, payment.Status);
        Assert.Equal("Card was declined.", payment.FailureReason);

        Assert.Equal(PaymentStatuses.Failed, order.PaymentStatus);
        Assert.Equal(OrderStatuses.Pending, order.Status);

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleStripeWebhookAsync_Should_Ignore_Unhandled_Event()
    {
        var stripeEvent = new Event
        {
            Type = "charge.refunded",
            Data = new EventData()
        };

        var result = await _service.HandleStripeWebhookAsync(
            new HandleStripeWebhookCommand(stripeEvent),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.True(result.Data);

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}