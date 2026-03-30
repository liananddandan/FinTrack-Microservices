using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;
using TransactionService.Api.Payments.Contracts;
using TransactionService.Api.Payments.Controllers;
using TransactionService.Application.Payments.Abstractions;
using TransactionService.Application.Payments.Commands;
using TransactionService.Application.Payments.Dtos;
using TransactionService.Application.Payments.Queries;
using TransactionService.Domain.Constants;

namespace TransactionService.Tests.Unit.Payments;

public class PaymentControllerTests
{
    private readonly Mock<IPaymentService> _paymentServiceMock;
    private readonly PaymentController _controller;

    public PaymentControllerTests()
    {
        _paymentServiceMock = new Mock<IPaymentService>();
        _controller = new PaymentController(_paymentServiceMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnOk_WhenPaymentCreated()
    {
        var orderPublicId = Guid.NewGuid();

        var request = new CreatePaymentRequest
        {
            OrderPublicId = orderPublicId,
            Provider = PaymentProviders.Stripe,
            PaymentMethod = PaymentMethods.Card
        };

        var dto = new PaymentDto
        {
            PaymentPublicId = Guid.NewGuid(),
            OrderPublicId = orderPublicId,
            Provider = PaymentProviders.Stripe,
            PaymentMethod = PaymentMethods.Card,
            Status = PaymentStatuses.Pending,
            Amount = 25.50m,
            Currency = "NZD",
            ProviderPaymentReference = "pi_test_123",
            ProviderClientSecret = "secret_123",
            CreatedAt = DateTime.UtcNow
        };

        _paymentServiceMock
            .Setup(x => x.CreateAsync(
                It.Is<CreatePaymentCommand>(c =>
                    c.OrderPublicId == request.OrderPublicId &&
                    c.Provider == request.Provider &&
                    c.PaymentMethod == request.PaymentMethod),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<PaymentDto>.Ok(
                dto,
                "Payment.CreateSuccess",
                "Payment created successfully."));

        var result = await _controller.CreateAsync(request, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<PaymentDto>>().Subject;

        response.Code.Should().Be("Payment.CreateSuccess");
        response.Data.Should().NotBeNull();
        response.Data!.OrderPublicId.Should().Be(orderPublicId);
        response.Data.Status.Should().Be(PaymentStatuses.Pending);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnBadRequest_WhenServiceFails()
    {
        var request = new CreatePaymentRequest
        {
            OrderPublicId = Guid.NewGuid(),
            Provider = PaymentProviders.Stripe,
            PaymentMethod = PaymentMethods.Card
        };

        _paymentServiceMock
            .Setup(x => x.CreateAsync(
                It.IsAny<CreatePaymentCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<PaymentDto>.Fail(
                "Payment.CreateFailed",
                "Failed to create payment."));

        var result = await _controller.CreateAsync(request, CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequest.Value.Should().BeOfType<ApiResponse<PaymentDto>>().Subject;

        response.Code.Should().Be("Payment.CreateFailed");
    }

    [Fact]
    public async Task GetByOrderAsync_ShouldReturnOk_WhenPaymentExists()
    {
        var orderPublicId = Guid.NewGuid();

        var dto = new PaymentDto
        {
            PaymentPublicId = Guid.NewGuid(),
            OrderPublicId = orderPublicId,
            Provider = PaymentProviders.Stripe,
            PaymentMethod = PaymentMethods.Card,
            Status = PaymentStatuses.Pending,
            Amount = 12.30m,
            Currency = "NZD",
            CreatedAt = DateTime.UtcNow
        };

        _paymentServiceMock
            .Setup(x => x.GetByOrderAsync(
                It.Is<GetPaymentByOrderQuery>(q => q.OrderPublicId == orderPublicId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<PaymentDto>.Ok(
                dto,
                "Payment.GetSuccess",
                "Payment retrieved successfully."));

        var result = await _controller.GetByOrderAsync(orderPublicId, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<PaymentDto>>().Subject;

        response.Code.Should().Be("Payment.GetSuccess");
        response.Data!.OrderPublicId.Should().Be(orderPublicId);
    }

    [Fact]
    public async Task GetByOrderAsync_ShouldReturnNotFound_WhenPaymentDoesNotExist()
    {
        var orderPublicId = Guid.NewGuid();

        _paymentServiceMock
            .Setup(x => x.GetByOrderAsync(
                It.Is<GetPaymentByOrderQuery>(q => q.OrderPublicId == orderPublicId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<PaymentDto>.Fail(
                "Payment.NotFound",
                "Payment not found."));

        var result = await _controller.GetByOrderAsync(orderPublicId, CancellationToken.None);

        var notFound = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = notFound.Value.Should().BeOfType<ApiResponse<PaymentDto>>().Subject;

        response.Code.Should().Be("Payment.NotFound");
    }
}