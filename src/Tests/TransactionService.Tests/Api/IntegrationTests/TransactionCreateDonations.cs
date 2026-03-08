using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;
using TransactionService.Api.Controllers;
using TransactionService.Application.Commands;
using TransactionService.Application.Common.DTOs;

namespace TransactionService.Tests.Api.IntegrationTests;

public class TransactionCreateDonations
{
    [Fact]
    public async Task CreateDonationAsync_Should_Return_Unauthorized_When_Claims_Are_Missing()
    {
        var mediatorMock = new Mock<IMediator>();
        var sut = new TransactionsController(mediatorMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var request = new CreateDonationRequest
        {
            Title = "Donation",
            Amount = 100,
            Currency = "NZD"
        };

        var result = await sut.CreateDonationAsync(request, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task CreateDonationAsync_Should_Return_Ok_When_Command_Succeeds()
    {
        var mediatorMock = new Mock<IMediator>();

        mediatorMock
            .Setup(x => x.Send(It.IsAny<CreateDonationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<CreateTransactionResult>.Ok(
                new CreateTransactionResult
                {
                    TransactionPublicId = Guid.NewGuid().ToString(),
                    TenantPublicId = Guid.NewGuid().ToString(),
                    TenantName = "Demo School",
                    Type = "Donation",
                    Amount = 100,
                    Currency = "NZD",
                    Status = "Completed",
                    PaymentStatus = "Succeeded",
                    PaymentReference = "MOCK-PAY-001"
                },
                ResultCodes.Transaction.TransactionCreateSuccess,
                "Donation transaction processed successfully."));

        var sut = new TransactionsController(mediatorMock.Object);
        sut.ControllerContext = BuildControllerContext(
            tenantPublicId: Guid.NewGuid().ToString(),
            userPublicId: Guid.NewGuid().ToString());

        var request = new CreateDonationRequest
        {
            Title = "Donation",
            Description = "Monthly support",
            Amount = 100,
            Currency = "NZD"
        };

        var result = await sut.CreateDonationAsync(request, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = okResult.Value.Should().BeOfType<ApiResponse<CreateTransactionResult>>().Subject;

        payload.Code.Should().Be(ResultCodes.Transaction.TransactionCreateSuccess);
        payload.Data.Should().NotBeNull();
        payload.Data!.PaymentStatus.Should().Be("Succeeded");
    }

    [Fact]
    public async Task CreateDonationAsync_Should_Return_BadRequest_When_Command_Fails()
    {
        var mediatorMock = new Mock<IMediator>();

        mediatorMock
            .Setup(x => x.Send(It.IsAny<CreateDonationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<CreateTransactionResult>.Fail(
                ResultCodes.Transaction.TransactionCreateFailed,
                "Amount must be greater than zero."));

        var sut = new TransactionsController(mediatorMock.Object);
        sut.ControllerContext = BuildControllerContext(
            tenantPublicId: Guid.NewGuid().ToString(),
            userPublicId: Guid.NewGuid().ToString());

        var request = new CreateDonationRequest
        {
            Title = "Donation",
            Amount = 0,
            Currency = "NZD"
        };

        var result = await sut.CreateDonationAsync(request, CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var payload = badRequest.Value.Should().BeOfType<ApiResponse<CreateTransactionResult>>().Subject;

        payload.Code.Should().Be(ResultCodes.Transaction.TransactionCreateFailed);
        payload.Data.Should().BeNull();
    }

    private static ControllerContext BuildControllerContext(string tenantPublicId, string userPublicId)
    {
        var claims = new List<Claim>
        {
            new("tenant", tenantPublicId),
            new("sub", userPublicId)
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }
}