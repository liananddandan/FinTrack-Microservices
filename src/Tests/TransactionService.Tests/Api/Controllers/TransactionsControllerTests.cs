using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;
using System.Security.Claims;
using SharedKernel.Common.DTOs.Auth;
using TransactionService.Api.Transaction.Contracts;
using TransactionService.Api.Transaction.Controllers;
using TransactionService.Application.Transactions.Commands;
using TransactionService.Application.Transactions.Queries;
using Xunit;

namespace TransactionService.Tests.Api.Controllers;

public partial class TransactionsControllerTests
{
    [Fact]
    public async Task GetTransactionDetailAsync_Should_Return_Unauthorized_When_Tenant_Claim_Is_Missing()
    {
        var mediatorMock = new Mock<IMediator>();

        var sut = new TransactionsController(mediatorMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(JwtClaimNames.UserId, Guid.NewGuid().ToString())
                    ], "TestAuthType"))
                }
            }
        };

        var result = await sut.GetTransactionDetailAsync(Guid.NewGuid().ToString(), CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetTransactionDetailAsync_Should_Return_Unauthorized_When_User_Claim_Is_Missing()
    {
        var mediatorMock = new Mock<IMediator>();

        var sut = new TransactionsController(mediatorMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(JwtClaimNames.Tenant, Guid.NewGuid().ToString())
                    ], "TestAuthType"))
                }
            }
        };

        var result = await sut.GetTransactionDetailAsync(Guid.NewGuid().ToString(), CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetTransactionDetailAsync_Should_Return_Ok_When_Query_Succeeds()
    {
        var mediatorMock = new Mock<IMediator>();

        mediatorMock
            .Setup(x => x.Send(It.IsAny<GetTransactionDetailQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TransactionDetailDto>.Ok(
                new TransactionDetailDto
                {
                    TransactionPublicId = Guid.NewGuid().ToString(),
                    TenantPublicId = Guid.NewGuid().ToString(),
                    TenantName = "Demo Tenant",
                    Type = "Donation",
                    Title = "Support Donation",
                    Description = "Monthly support",
                    Amount = 100,
                    Currency = "NZD",
                    Status = "Completed",
                    PaymentStatus = "Succeeded",
                    RiskStatus = "NotChecked",
                    CreatedByUserPublicId = Guid.NewGuid().ToString(),
                    CreatedAtUtc = DateTime.UtcNow
                },
                ResultCodes.Transaction.TransactionQuerySuccess,
                "Transaction retrieved successfully."));

        var tenantId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        var transactionId = Guid.NewGuid().ToString();

        var sut = new TransactionsController(mediatorMock.Object)
        {
            ControllerContext = BuildControllerContextForDetail(tenantId, userId, "Member")
        };

        var result = await sut.GetTransactionDetailAsync(transactionId, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = okResult.Value.Should().BeOfType<ApiResponse<TransactionDetailDto>>().Subject;

        payload.Code.Should().Be(ResultCodes.Transaction.TransactionQuerySuccess);
        payload.Data.Should().NotBeNull();
        payload.Data!.Title.Should().Be("Support Donation");

        mediatorMock.Verify(x => x.Send(
                It.Is<GetTransactionDetailQuery>(q =>
                    q.TenantPublicId == tenantId &&
                    q.UserPublicId == userId &&
                    q.Role == "Member" &&
                    q.TransactionPublicId == transactionId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetTransactionDetailAsync_Should_Return_BadRequest_When_Query_Fails()
    {
        var mediatorMock = new Mock<IMediator>();

        mediatorMock
            .Setup(x => x.Send(It.IsAny<GetTransactionDetailQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TransactionDetailDto>.Fail(
                ResultCodes.Transaction.TransactionNotFound,
                "Transaction not found."));

        var sut = new TransactionsController(mediatorMock.Object)
        {
            ControllerContext = BuildControllerContextForDetail(
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                "Member")
        };

        var result = await sut.GetTransactionDetailAsync(Guid.NewGuid().ToString(), CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var payload = badRequest.Value.Should().BeOfType<ApiResponse<TransactionDetailDto>>().Subject;

        payload.Code.Should().Be(ResultCodes.Transaction.TransactionNotFound);
        payload.Data.Should().BeNull();
    }

    private static ControllerContext BuildControllerContextForDetail(
        string tenantPublicId,
        string userPublicId,
        string role)
    {
        var claims = new List<Claim>
        {
            new(JwtClaimNames.Tenant, tenantPublicId),
            new(JwtClaimNames.UserId, userPublicId),
            new(ClaimTypes.Role, role)
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

    [Fact]
    public async Task GetTransactionsAsync_Should_Return_Unauthorized_When_Tenant_Claim_Is_Missing()
    {
        var mediatorMock = new Mock<IMediator>();

        var sut = new TransactionsController(mediatorMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            }
        };

        var result = await sut.GetTransactionsAsync(null, null, null, 1, 10, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetTransactionsAsync_Should_Return_Ok_When_Query_Succeeds()
    {
        var mediatorMock = new Mock<IMediator>();

        mediatorMock
            .Setup(x => x.Send(It.IsAny<GetTransactionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<PagedResult<TransactionListItemDto>>.Ok(
                new PagedResult<TransactionListItemDto>
                {
                    Items = [],
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                },
                ResultCodes.Transaction.TransactionQueryByPageSuccess,
                "ok"));

        var claims = new List<Claim>
        {
            new(JwtClaimNames.Tenant, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, "Admin")
        };

        var sut = new TransactionsController(mediatorMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"))
                }
            }
        };

        var result =
            await sut.GetTransactionsAsync("Donation", "Completed", "Succeeded", 1, 10, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetTransactionSummaryAsync_Should_Return_Unauthorized_When_Tenant_Claim_Is_Missing()
    {
        var mediatorMock = new Mock<IMediator>();

        var sut = new TransactionsController(mediatorMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            }
        };

        var result = await sut.GetTransactionSummaryAsync(CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetTransactionSummaryAsync_Should_Return_Ok_When_Query_Succeeds()
    {
        var mediatorMock = new Mock<IMediator>();

        mediatorMock
            .Setup(x => x.Send(It.IsAny<GetTransactionSummaryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TenantTransactionSummaryDto>.Ok(
                new TenantTransactionSummaryDto
                {
                    TenantPublicId = Guid.NewGuid().ToString(),
                    TenantName = "Demo Tenant",
                    CurrentBalance = 150,
                    TotalDonationAmount = 200,
                    TotalProcurementAmount = 50,
                    TotalTransactionCount = 3
                },
                ResultCodes.Transaction.TransactionQuerySuccess,
                "ok"));

        var claims = new List<Claim>
        {
            new(JwtClaimNames.Tenant, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, "Admin")
        };

        var sut = new TransactionsController(mediatorMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"))
                }
            }
        };

        var result = await sut.GetTransactionSummaryAsync(CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task CreateProcurementAsync_Should_Return_Unauthorized_When_Claims_Are_Missing()
    {
        var mediatorMock = new Mock<IMediator>();
        var sut = new TransactionsController(mediatorMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var result = await sut.CreateProcurementAsync(
            new CreateProcurementRequest
            {
                Title = "Buy Laptop",
                Amount = 100,
                Currency = "NZD"
            },
            CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task CreateProcurementAsync_Should_Return_Ok_When_Command_Succeeds()
    {
        var mediatorMock = new Mock<IMediator>();

        mediatorMock
            .Setup(x => x.Send(It.IsAny<CreateProcurementCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<CreateTransactionResult>.Ok(
                new CreateTransactionResult
                {
                    TransactionPublicId = Guid.NewGuid().ToString(),
                    TenantPublicId = Guid.NewGuid().ToString(),
                    TenantName = "Demo Tenant",
                    Type = "Procurement",
                    Amount = 100,
                    Currency = "NZD",
                    Status = "Draft",
                    PaymentStatus = "NotStarted"
                },
                ResultCodes.Transaction.ProcurementCreateSuccess,
                "ok"));

        var sut = new TransactionsController(mediatorMock.Object)
        {
            ControllerContext = BuildControllerContextForProcurement(
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                "Member")
        };

        var result = await sut.CreateProcurementAsync(
            new CreateProcurementRequest
            {
                Title = "Buy Laptop",
                Amount = 100,
                Currency = "NZD"
            },
            CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    private static ControllerContext BuildControllerContextForProcurement(
        string tenantPublicId,
        string userPublicId,
        string role)
    {
        var claims = new List<Claim>
        {
            new(JwtClaimNames.Tenant, tenantPublicId),
            new(JwtClaimNames.UserId, userPublicId),
            new(ClaimTypes.Role, role)
        };

        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"))
            }
        };
    }
}