using FluentAssertions;
using Moq;
using SharedKernel.Common.Results;
using TransactionService.Application.Common.Abstractions;
using TransactionService.Application.Orders.Abstractions;
using TransactionService.Application.Payments.Abstractions;
using TransactionService.Application.Payments.Commands;
using TransactionService.Application.Payments.Dtos;
using TransactionService.Application.Payments.Queries;
using TransactionService.Application.Payments.Services;
using TransactionService.Domain.Constants;
using TransactionService.Domain.Entities;
using Xunit;

namespace TransactionService.Tests.Unit.Payments;

public class PaymentServiceTests
{
    private readonly Mock<ICurrentTenantContext> _currentTenantContextMock;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<IPaymentGatewayResolver> _paymentGatewayResolverMock;
    private readonly Mock<IPaymentGateway> _paymentGatewayMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    private readonly PaymentService _service;

    private readonly Guid _tenantPublicId = Guid.NewGuid();

    public PaymentServiceTests()
    {
        _currentTenantContextMock = new Mock<ICurrentTenantContext>();
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _paymentGatewayResolverMock = new Mock<IPaymentGatewayResolver>();
        _paymentGatewayMock = new Mock<IPaymentGateway>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _currentTenantContextMock.Setup(x => x.TenantPublicId).Returns(_tenantPublicId);

        _service = new PaymentService(
            _currentTenantContextMock.Object,
            _orderRepositoryMock.Object,
            _paymentRepositoryMock.Object,
            _paymentGatewayResolverMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreatePaymentSuccessfully_WhenOrderIsValid()
    {
        var orderPublicId = Guid.NewGuid();

        var order = new Order
        {
            Id = 10,
            PublicId = orderPublicId,
            TenantPublicId = _tenantPublicId,
            OrderNumber = "AKC-0001",
            TotalAmount = 18.50m,
            Status = OrderStatuses.Pending,
            PaymentStatus = PaymentStatuses.NotStarted
        };

        Payment? savedPayment = null;

        _orderRepositoryMock
            .Setup(x => x.GetByPublicIdAsync(_tenantPublicId, orderPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _paymentRepositoryMock
            .Setup(x => x.GetByOrderIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        _paymentRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Callback<Payment, CancellationToken>((payment, _) => savedPayment = payment)
            .Returns(Task.CompletedTask);

        _paymentGatewayResolverMock
            .Setup(x => x.Resolve(PaymentProviders.Stripe))
            .Returns(_paymentGatewayMock.Object);

        _paymentGatewayMock
            .SetupGet(x => x.Provider)
            .Returns(PaymentProviders.Stripe);

        _paymentGatewayMock
            .Setup(x => x.CreatePaymentAsync(
                It.IsAny<CreatePaymentGatewayRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreatePaymentGatewayResult
            {
                ProviderPaymentReference = "pi_test_123",
                ClientSecret = "secret_test_123",
                Status = "requires_payment_method"
            });

        var command = new CreatePaymentCommand(
            orderPublicId,
            PaymentProviders.Stripe,
            PaymentMethods.Card);

        var result = await _service.CreateAsync(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Code.Should().Be(ResultCodes.Payment.CreateSuccess);
        result.Data.Should().NotBeNull();
        result.Data!.OrderPublicId.Should().Be(orderPublicId);
        result.Data.Provider.Should().Be(PaymentProviders.Stripe);
        result.Data.PaymentMethod.Should().Be(PaymentMethods.Card);
        result.Data.Status.Should().Be(PaymentStatuses.Pending);
        result.Data.ProviderPaymentReference.Should().Be("pi_test_123");

        savedPayment.Should().NotBeNull();
        savedPayment!.OrderId.Should().Be(order.Id);
        savedPayment.Amount.Should().Be(order.TotalAmount);
        savedPayment.Provider.Should().Be(PaymentProviders.Stripe);
        savedPayment.Status.Should().Be(PaymentStatuses.Pending);

        order.PaymentStatus.Should().Be(PaymentStatuses.Pending);
        order.Status.Should().Be(OrderStatuses.Pending);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenOrderNotFound()
    {
        var orderPublicId = Guid.NewGuid();

        _orderRepositoryMock
            .Setup(x => x.GetByPublicIdAsync(_tenantPublicId, orderPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var command = new CreatePaymentCommand(
            orderPublicId,
            PaymentProviders.Stripe,
            PaymentMethods.Card);

        var result = await _service.CreateAsync(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Order.NotFound);
    }

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenOrderAlreadyPaid()
    {
        var orderPublicId = Guid.NewGuid();

        var order = new Order
        {
            Id = 1,
            PublicId = orderPublicId,
            TenantPublicId = _tenantPublicId,
            OrderNumber = "AKC-0002",
            TotalAmount = 10.00m,
            PaymentStatus = PaymentStatuses.Paid
        };

        _orderRepositoryMock
            .Setup(x => x.GetByPublicIdAsync(_tenantPublicId, orderPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new CreatePaymentCommand(
            orderPublicId,
            PaymentProviders.Stripe,
            PaymentMethods.Card);

        var result = await _service.CreateAsync(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Order.InvalidStatus);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnExistingPendingPayment_WhenPaymentAlreadyExists()
    {
        var orderPublicId = Guid.NewGuid();

        var order = new Order
        {
            Id = 1,
            PublicId = orderPublicId,
            TenantPublicId = _tenantPublicId,
            OrderNumber = "AKC-0003",
            TotalAmount = 11.20m,
            PaymentStatus = PaymentStatuses.Pending
        };

        var existingPayment = new Payment
        {
            Id = 100,
            PublicId = Guid.NewGuid(),
            TenantPublicId = _tenantPublicId,
            OrderId = order.Id,
            Provider = PaymentProviders.Stripe,
            PaymentMethod = PaymentMethods.Card,
            Status = PaymentStatuses.Pending,
            Amount = 11.20m,
            Currency = "NZD",
            IdempotencyKey = "payment-existing",
            ProviderPaymentReference = "pi_existing_123",
            ProviderClientSecret = "secret_existing",
            CreatedAt = DateTime.UtcNow
        };

        _orderRepositoryMock
            .Setup(x => x.GetByPublicIdAsync(_tenantPublicId, orderPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _paymentRepositoryMock
            .Setup(x => x.GetByOrderIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayment);

        var command = new CreatePaymentCommand(
            orderPublicId,
            PaymentProviders.Stripe,
            PaymentMethods.Card);

        var result = await _service.CreateAsync(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data!.PaymentPublicId.Should().Be(existingPayment.PublicId);
        result.Data.ProviderPaymentReference.Should().Be("pi_existing_123");

        _paymentGatewayResolverMock.Verify(
            x => x.Resolve(It.IsAny<string>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetByOrderAsync_ShouldReturnPayment_WhenPaymentExists()
    {
        var orderPublicId = Guid.NewGuid();

        var payment = new Payment
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            TenantPublicId = _tenantPublicId,
            Provider = PaymentProviders.Stripe,
            PaymentMethod = PaymentMethods.Card,
            Status = PaymentStatuses.Pending,
            Amount = 9.90m,
            Currency = "NZD",
            CreatedAt = DateTime.UtcNow,
            Order = new Order
            {
                PublicId = orderPublicId
            }
        };

        _paymentRepositoryMock
            .Setup(x => x.GetByOrderPublicIdAsync(_tenantPublicId, orderPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var result = await _service.GetByOrderAsync(
            new GetPaymentByOrderQuery(orderPublicId),
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Code.Should().Be(ResultCodes.Payment.GetSuccess);
        result.Data.Should().NotBeNull();
        result.Data!.OrderPublicId.Should().Be(orderPublicId);
    }

    [Fact]
    public async Task GetByOrderAsync_ShouldFail_WhenPaymentNotFound()
    {
        var orderPublicId = Guid.NewGuid();

        _paymentRepositoryMock
            .Setup(x => x.GetByOrderPublicIdAsync(_tenantPublicId, orderPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var result = await _service.GetByOrderAsync(
            new GetPaymentByOrderQuery(orderPublicId),
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Payment.NotFound);
    }
}