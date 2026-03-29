using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;
using TransactionService.Api.Orders.Contracts;
using TransactionService.Api.Orders.Controllers;
using TransactionService.Api.Transaction.Contracts;
using TransactionService.Application.Orders.Commands;
using TransactionService.Application.Orders.Dtos;
using TransactionService.Application.Orders.Queries;
using Xunit;

namespace TransactionService.Tests.Unit.Orders;

public class OrdersControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly OrdersController _controller;

    public OrdersControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new OrdersController(_mediatorMock.Object);
    }

    [Fact]
    public async Task Create_ShouldReturnOk_WhenMediatorReturnsSuccess()
    {
        var request = new CreateOrderRequest(
            "Emily",
            "0211234567",
            "Cash",
            new List<CreateOrderItemRequest>
            {
                new(Guid.NewGuid(), 2, "Less sugar")
            });

        var response = ServiceResult<OrderDto>.Ok(
            new OrderDto
            {
                PublicId = Guid.NewGuid(),
                OrderNumber = "ORD-20260329-0001",
                CustomerName = "Emily",
                CustomerPhone = "0211234567",
                CreatedByUserPublicId = Guid.NewGuid(),
                CreatedByUserNameSnapshot = "Test User",
                SubtotalAmount = 11m,
                GstRate = 0.15m,
                GstAmount = 1.65m,
                DiscountAmount = 0m,
                TotalAmount = 12.65m,
                Status = "Completed",
                PaymentStatus = "Paid",
                PaymentMethod = "Cash",
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItemDto>
                {
                    new()
                    {
                        ProductPublicId = Guid.NewGuid(),
                        ProductNameSnapshot = "Latte",
                        UnitPrice = 5.5m,
                        Quantity = 2,
                        LineTotal = 11m,
                        Notes = "Less sugar"
                    }
                }
            },
            ResultCodes.Order.CreateSuccess);

        _mediatorMock.Setup(x => x.Send(
                It.IsAny<CreateOrderCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.Create(request, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();

        var ok = (OkObjectResult)result;
        ok.Value.Should().BeOfType<ApiResponse<OrderDto>>();

        var body = (ApiResponse<OrderDto>)ok.Value!;
        body.Code.Should().Be(ResultCodes.Order.CreateSuccess);
        body.Data.Should().NotBeNull();
        body.Data!.OrderNumber.Should().Be("ORD-20260329-0001");
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenMediatorReturnsFailure()
    {
        var request = new CreateOrderRequest(
            null,
            null,
            "Cash",
            new List<CreateOrderItemRequest>());

        var response = ServiceResult<OrderDto>.Fail(
            ResultCodes.Order.CreateParameterError,
            "Order items are required.");

        _mediatorMock.Setup(x => x.Send(
                It.IsAny<CreateOrderCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.Create(request, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();

        var badRequest = (BadRequestObjectResult)result;
        badRequest.Value.Should().BeOfType<ApiResponse<OrderDto>>();

        var body = (ApiResponse<OrderDto>)badRequest.Value!;
        body.Code.Should().Be(ResultCodes.Order.CreateParameterError);
        body.Message.Should().Be("Order items are required.");
        body.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenMediatorReturnsSuccess()
    {
        var orderPublicId = Guid.NewGuid();

        var response = ServiceResult<OrderDto>.Ok(
            new OrderDto
            {
                PublicId = orderPublicId,
                OrderNumber = "ORD-20260329-0001",
                Status = "Completed",
                PaymentStatus = "Paid",
                PaymentMethod = "Cash",
                CreatedAt = DateTime.UtcNow
            },
            ResultCodes.Order.GetByIdSuccess);

        _mediatorMock.Setup(x => x.Send(
                It.IsAny<GetOrderByPublicIdQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.GetById(orderPublicId, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();

        var ok = (OkObjectResult)result;
        ok.Value.Should().BeOfType<ApiResponse<OrderDto>>();

        var body = (ApiResponse<OrderDto>)ok.Value!;
        body.Code.Should().Be(ResultCodes.Order.GetByIdSuccess);
        body.Data.Should().NotBeNull();
        body.Data!.PublicId.Should().Be(orderPublicId);
    }

    [Fact]
    public async Task GetPaged_ShouldReturnOk_WhenMediatorReturnsSuccess()
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
                    TotalAmount = 12.65m,
                    Status = "Completed",
                    PaymentStatus = "Paid",
                    PaymentMethod = "Cash",
                    CreatedByUserNameSnapshot = "Test User",
                    CreatedAt = DateTime.UtcNow
                }
            },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 20
        };

        var response = ServiceResult<PagedResult<OrderListItemDto>>.Ok(
            pagedResult,
            ResultCodes.Order.GetPagedSuccess);

        _mediatorMock.Setup(x => x.Send(
                It.IsAny<GetOrdersQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.GetPaged(
            false,
            null,
            null,
            null,
            1,
            20,
            CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();

        var ok = (OkObjectResult)result;
        ok.Value.Should().BeOfType<ApiResponse<PagedResult<OrderListItemDto>>>();

        var body = (ApiResponse<PagedResult<OrderListItemDto>>)ok.Value!;
        body.Code.Should().Be(ResultCodes.Order.GetPagedSuccess);
        body.Data.Should().NotBeNull();
        body.Data!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Cancel_ShouldReturnOk_WhenMediatorReturnsSuccess()
    {
        var orderPublicId = Guid.NewGuid();

        var response = ServiceResult<bool>.Ok(
            true,
            ResultCodes.Order.CancelSuccess);

        _mediatorMock.Setup(x => x.Send(
                It.IsAny<CancelOrderCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.Cancel(orderPublicId, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();

        var ok = (OkObjectResult)result;
        ok.Value.Should().BeOfType<ApiResponse<bool>>();

        var body = (ApiResponse<bool>)ok.Value!;
        body.Code.Should().Be(ResultCodes.Order.CancelSuccess);
        body.Data.Should().BeTrue();
    }

    [Fact]
    public async Task Cancel_ShouldReturnBadRequest_WhenMediatorReturnsFailure()
    {
        var orderPublicId = Guid.NewGuid();

        var response = ServiceResult<bool>.Fail(
            ResultCodes.Order.NotFound,
            "Order not found.");

        _mediatorMock.Setup(x => x.Send(
                It.IsAny<CancelOrderCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.Cancel(orderPublicId, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();

        var badRequest = (BadRequestObjectResult)result;
        badRequest.Value.Should().BeOfType<ApiResponse<bool>>();

        var body = (ApiResponse<bool>)badRequest.Value!;
        body.Code.Should().Be(ResultCodes.Order.NotFound);
        body.Message.Should().Be("Order not found.");
        body.Data.Should().BeFalse();
    }
    
    [Fact]
    public async Task GetSummary_ShouldReturnOk_WhenMediatorReturnsSuccess()
    {
        var response = ServiceResult<OrderSummaryDto>.Ok(
            new OrderSummaryDto
            {
                OrderCount = 6,
                TotalRevenue = 150.00m,
                AverageOrderValue = 25.00m,
                CancelledOrderCount = 1
            },
            ResultCodes.Order.GetSummarySuccess);

        _mediatorMock.Setup(x => x.Send(
                It.IsAny<GetOrderSummaryQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.GetSummary(
            false,
            null,
            null,
            CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();

        var ok = (OkObjectResult)result;
        ok.Value.Should().BeOfType<ApiResponse<OrderSummaryDto>>();

        var body = (ApiResponse<OrderSummaryDto>)ok.Value!;
        body.Code.Should().Be(ResultCodes.Order.GetSummarySuccess);
        body.Data.Should().NotBeNull();
        body.Data!.OrderCount.Should().Be(6);
        body.Data.TotalRevenue.Should().Be(150.00m);
    }
    
    [Fact]
    public async Task GetSummary_ShouldReturnBadRequest_WhenMediatorReturnsFailure()
    {
        var response = ServiceResult<OrderSummaryDto>.Fail(
            ResultCodes.Forbidden,
            "Tenant context is missing.");

        _mediatorMock.Setup(x => x.Send(
                It.IsAny<GetOrderSummaryQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.GetSummary(
            false,
            null,
            null,
            CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();

        var badRequest = (BadRequestObjectResult)result;
        badRequest.Value.Should().BeOfType<ApiResponse<OrderSummaryDto>>();

        var body = (ApiResponse<OrderSummaryDto>)badRequest.Value!;
        body.Code.Should().Be(ResultCodes.Forbidden);
        body.Data.Should().BeNull();
    }
}