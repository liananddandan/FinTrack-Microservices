using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SharedKernel.Common.Results;
using TransactionService.Api.Payments.Contracts;
using TransactionService.Api.Payments.Controllers;
using TransactionService.Application.Payments.Commands;
using TransactionService.Application.Payments.Dtos;
using TransactionService.Application.Payments.Queries;
using TransactionService.Domain.Constants;
using Xunit;

namespace TransactionService.Tests.Unit.Payments;

public class PaymentControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly PaymentController _controller;

    public PaymentControllerTests()
    {
        _controller = new PaymentController(_mediator.Object);
    }

    [Fact]
    public async Task CreateAsync_Should_Send_Command_And_Return_ActionResult()
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
            Amount = 12.50m,
            Currency = "NZD"
        };

        _mediator
            .Setup(x => x.Send(
                It.Is<CreatePaymentCommand>(c =>
                    c.OrderPublicId == request.OrderPublicId &&
                    c.Provider == request.Provider &&
                    c.PaymentMethod == request.PaymentMethod),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<PaymentDto>.Ok(
                dto,
                ResultCodes.Payment.CreateSuccess,
                "Payment created successfully."));

        var result = await _controller.CreateAsync(request, CancellationToken.None);

        Assert.IsAssignableFrom<IActionResult>(result);

        _mediator.Verify(x => x.Send(
            It.Is<CreatePaymentCommand>(c =>
                c.OrderPublicId == request.OrderPublicId &&
                c.Provider == request.Provider &&
                c.PaymentMethod == request.PaymentMethod),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByOrderAsync_Should_Send_Query_And_Return_ActionResult()
    {
        var orderPublicId = Guid.NewGuid();

        var dto = new PaymentDto
        {
            PaymentPublicId = Guid.NewGuid(),
            OrderPublicId = orderPublicId,
            Provider = PaymentProviders.Stripe,
            PaymentMethod = PaymentMethods.Card,
            Status = PaymentStatuses.Pending,
            Amount = 10m,
            Currency = "NZD"
        };

        _mediator
            .Setup(x => x.Send(
                It.Is<GetPaymentByOrderQuery>(q => q.OrderPublicId == orderPublicId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<PaymentDto>.Ok(
                dto,
                ResultCodes.Payment.GetSuccess,
                "Payment retrieved successfully."));

        var result = await _controller.GetByOrderAsync(orderPublicId, CancellationToken.None);

        Assert.IsAssignableFrom<IActionResult>(result);

        _mediator.Verify(x => x.Send(
            It.Is<GetPaymentByOrderQuery>(q => q.OrderPublicId == orderPublicId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}