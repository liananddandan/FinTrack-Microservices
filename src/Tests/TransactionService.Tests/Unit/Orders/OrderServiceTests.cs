using FluentAssertions;
using Moq;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;
using TransactionService.Api.Transaction.Contracts;
using TransactionService.Application.Common.Abstractions;
using TransactionService.Application.Orders.Abstractions;
using TransactionService.Application.Orders.Commands;
using TransactionService.Application.Orders.Dtos;
using TransactionService.Application.Orders.Queries;
using TransactionService.Application.Orders.Services;
using TransactionService.Domain.Constants;
using TransactionService.Domain.Entities;
using Xunit;

namespace TransactionService.Tests.Unit.Orders;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<ICurrentTenantContext> _currentTenantContextMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly OrderService _service;

    private readonly Guid _tenantPublicId = Guid.NewGuid();
    private readonly Guid _userPublicId = Guid.NewGuid();

    public OrderServiceTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _currentTenantContextMock = new Mock<ICurrentTenantContext>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _currentTenantContextMock.Setup(x => x.TenantPublicId).Returns(_tenantPublicId);
        _currentTenantContextMock.Setup(x => x.UserPublicId).Returns(_userPublicId);
        _currentTenantContextMock.Setup(x => x.UserName).Returns("Test User");
        _currentTenantContextMock.Setup(x => x.UserEmail).Returns("test@example.com");
        _currentTenantContextMock.Setup(x => x.IsAuthenticated).Returns(true);

        _service = new OrderService(
            _orderRepositoryMock.Object,
            _currentTenantContextMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnFailure_WhenTenantContextMissing()
    {
        _currentTenantContextMock.Setup(x => x.TenantPublicId).Returns(Guid.Empty);

        var command = new CreateOrderCommand(
            "Emily",
            "0211234567",
            PaymentMethods.Cash,
            new List<CreateOrderItemCommand>
            {
                new(Guid.NewGuid(), 1, null)
            });

        var result = await _service.CreateAsync(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Forbidden);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnFailure_WhenItemsAreEmpty()
    {
        var command = new CreateOrderCommand(
            "Emily",
            "0211234567",
            PaymentMethods.Cash,
            new List<CreateOrderItemCommand>());

        var result = await _service.CreateAsync(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Order.CreateParameterError);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnFailure_WhenQuantityIsInvalid()
    {
        var command = new CreateOrderCommand(
            "Emily",
            "0211234567",
            PaymentMethods.Cash,
            new List<CreateOrderItemCommand>
            {
                new(Guid.NewGuid(), 0, null)
            });

        var result = await _service.CreateAsync(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Order.CreateParameterError);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnFailure_WhenProductDoesNotExist()
    {
        var productPublicId = Guid.NewGuid();

        _orderRepositoryMock.Setup(x => x.GetProductsByPublicIdsAsync(
                _tenantPublicId,
                It.IsAny<List<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());

        var command = new CreateOrderCommand(
            "Emily",
            "0211234567",
            PaymentMethods.Cash,
            new List<CreateOrderItemCommand>
            {
                new(productPublicId, 2, null)
            });

        var result = await _service.CreateAsync(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Order.ProductNotFound);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateOrderSuccessfully()
    {
        var productPublicId = Guid.NewGuid();

        var product = new Product
        {
            Id = 1,
            PublicId = productPublicId,
            TenantPublicId = _tenantPublicId,
            CategoryId = 10,
            Name = "Latte",
            Price = 5.50m,
            IsAvailable = true
        };

        _orderRepositoryMock.Setup(x => x.GetProductsByPublicIdsAsync(
                _tenantPublicId,
                It.IsAny<List<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product });

        _orderRepositoryMock.Setup(x => x.CountTodayOrdersAsync(
                _tenantPublicId,
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        Order? savedOrder = null;

        _orderRepositoryMock.Setup(x => x.AddAsync(
                It.IsAny<Order>(),
                It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((order, _) => savedOrder = order)
            .Returns(Task.CompletedTask);

        var command = new CreateOrderCommand(
            "Emily",
            "0211234567",
            PaymentMethods.Cash,
            new List<CreateOrderItemCommand>
            {
                new(productPublicId, 2, "Less sugar")
            });

        var result = await _service.CreateAsync(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Code.Should().Be(ResultCodes.Order.CreateSuccess);
        result.Data.Should().NotBeNull();
        result.Data!.CustomerName.Should().Be("Emily");
        result.Data.CreatedByUserPublicId.Should().Be(_userPublicId);
        result.Data.CreatedByUserNameSnapshot.Should().Be("Test User");
        result.Data.SubtotalAmount.Should().Be(11.00m);
        result.Data.GstAmount.Should().Be(1.65m);
        result.Data.TotalAmount.Should().Be(12.65m);
        result.Data.Items.Should().HaveCount(1);
        result.Data.Items[0].ProductNameSnapshot.Should().Be("Latte");
        result.Data.Items[0].Quantity.Should().Be(2);
        result.Data.Items[0].LineTotal.Should().Be(11.00m);

        savedOrder.Should().NotBeNull();
        savedOrder!.OrderNumber.Should().StartWith("ORD-");
        savedOrder.Items.Should().HaveCount(1);
        savedOrder.Items.First().ProductNameSnapshot.Should().Be("Latte");
        savedOrder.Items.First().Quantity.Should().Be(2);
        savedOrder.Items.First().LineTotal.Should().Be(11.00m);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByPublicIdAsync_ShouldReturnFailure_WhenOrderNotFound()
    {
        var orderPublicId = Guid.NewGuid();

        _orderRepositoryMock.Setup(x => x.GetByPublicIdAsync(
                _tenantPublicId,
                orderPublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var result = await _service.GetByPublicIdAsync(
            new GetOrderByPublicIdQuery(orderPublicId),
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Order.NotFound);
    }

    [Fact]
    public async Task GetByPublicIdAsync_ShouldReturnOrderSuccessfully()
    {
        var order = new Order
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            TenantPublicId = _tenantPublicId,
            OrderNumber = "ORD-20260329-0001",
            CustomerName = "Emily",
            CustomerPhone = "0211234567",
            CreatedByUserPublicId = _userPublicId,
            CreatedByUserNameSnapshot = "Test User",
            SubtotalAmount = 10m,
            GstRate = 0.15m,
            GstAmount = 1.5m,
            DiscountAmount = 0m,
            TotalAmount = 11.5m,
            Status = OrderStatuses.Completed,
            PaymentStatus = PaymentStatuses.Paid,
            PaymentMethod = PaymentMethods.Cash,
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>
            {
                new()
                {
                    ProductPublicId = Guid.NewGuid(),
                    ProductNameSnapshot = "Latte",
                    Quantity = 2,
                    UnitPrice = 5m,
                    LineTotal = 10m
                }
            }
        };

        _orderRepositoryMock.Setup(x => x.GetByPublicIdAsync(
                _tenantPublicId,
                order.PublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _service.GetByPublicIdAsync(
            new GetOrderByPublicIdQuery(order.PublicId),
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Code.Should().Be(ResultCodes.Order.GetByIdSuccess);
        result.Data.Should().NotBeNull();
        result.Data!.OrderNumber.Should().Be(order.OrderNumber);
        result.Data.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedOrders()
    {
        var pagedResult = new PagedResult<OrderListItemDto>
        {
            Items = new List<OrderListItemDto>
            {
                new()
                {
                    PublicId = Guid.NewGuid(),
                    OrderNumber = "ORD-20260329-0001",
                    CustomerName = "Emily",
                    TotalAmount = 11.5m,
                    Status = OrderStatuses.Completed,
                    PaymentStatus = PaymentStatuses.Paid,
                    PaymentMethod = PaymentMethods.Cash,
                    CreatedByUserNameSnapshot = "Test User",
                    CreatedAt = DateTime.UtcNow
                }
            },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 20
        };

        _orderRepositoryMock.Setup(x => x.GetPagedAsync(
                _tenantPublicId,
                null,
                null,
                null,
                null,
                1,
                20,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var result = await _service.GetPagedAsync(
            new GetOrdersQuery(false, null, null, null, 1, 20),
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Code.Should().Be(ResultCodes.Order.GetPagedSuccess);
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(1);
        result.Data.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldUseCurrentUserFilter_WhenCreatedByMeIsTrue()
    {
        var pagedResult = new PagedResult<OrderListItemDto>
        {
            Items = new List<OrderListItemDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 20
        };

        _orderRepositoryMock.Setup(x => x.GetPagedAsync(
                _tenantPublicId,
                _userPublicId,
                null,
                null,
                null,
                1,
                20,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var result = await _service.GetPagedAsync(
            new GetOrdersQuery(true, null, null, null, 1, 20),
            CancellationToken.None);

        result.Success.Should().BeTrue();

        _orderRepositoryMock.Verify(x => x.GetPagedAsync(
            _tenantPublicId,
            _userPublicId,
            null,
            null,
            null,
            1,
            20,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_ShouldReturnFailure_WhenOrderNotFound()
    {
        var orderPublicId = Guid.NewGuid();

        _orderRepositoryMock.Setup(x => x.GetByPublicIdAsync(
                _tenantPublicId,
                orderPublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var result = await _service.CancelAsync(
            new CancelOrderCommand(orderPublicId),
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Order.NotFound);
    }

    [Fact]
    public async Task CancelAsync_ShouldReturnFailure_WhenOrderAlreadyCancelled()
    {
        var order = new Order
        {
            PublicId = Guid.NewGuid(),
            TenantPublicId = _tenantPublicId,
            OrderNumber = "ORD-20260329-0001",
            CreatedByUserPublicId = _userPublicId,
            CreatedByUserNameSnapshot = "Test User",
            SubtotalAmount = 10m,
            GstAmount = 1.5m,
            TotalAmount = 11.5m,
            Status = OrderStatuses.Cancelled,
            PaymentStatus = PaymentStatuses.Paid,
            PaymentMethod = PaymentMethods.Cash
        };

        _orderRepositoryMock.Setup(x => x.GetByPublicIdAsync(
                _tenantPublicId,
                order.PublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _service.CancelAsync(
            new CancelOrderCommand(order.PublicId),
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Order.AlreadyCancelled);
    }

    [Fact]
    public async Task CancelAsync_ShouldCancelOrderSuccessfully()
    {
        var order = new Order
        {
            PublicId = Guid.NewGuid(),
            TenantPublicId = _tenantPublicId,
            OrderNumber = "ORD-20260329-0001",
            CreatedByUserPublicId = _userPublicId,
            CreatedByUserNameSnapshot = "Test User",
            SubtotalAmount = 10m,
            GstAmount = 1.5m,
            TotalAmount = 11.5m,
            Status = OrderStatuses.Completed,
            PaymentStatus = PaymentStatuses.Paid,
            PaymentMethod = PaymentMethods.Cash,
            PaidAt = DateTime.UtcNow
        };

        _orderRepositoryMock.Setup(x => x.GetByPublicIdAsync(
                _tenantPublicId,
                order.PublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _service.CancelAsync(
            new CancelOrderCommand(order.PublicId),
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Code.Should().Be(ResultCodes.Order.CancelSuccess);
        result.Data.Should().BeTrue();

        order.Status.Should().Be(OrderStatuses.Cancelled);
        order.PaidAt.Should().BeNull();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}