using FluentAssertions;
using Moq;
using SharedKernel.Common.Results;
using TransactionService.Application.Abstractions;
using TransactionService.Application.Common.DTOs;
using TransactionService.Application.Queries;
using TransactionService.Application.QueryHandlers;
using Xunit;

namespace TransactionService.Tests.Application.QueryHandlers;

public class GetTransactionDetailQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Delegate_To_TransactionService()
    {
        var transactionServiceMock = new Mock<ITransactionService>();

        transactionServiceMock
            .Setup(x => x.GetTransactionDetailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TransactionDetailDto>.Ok(
                new TransactionDetailDto
                {
                    TransactionPublicId = Guid.NewGuid().ToString(),
                    TenantPublicId = Guid.NewGuid().ToString(),
                    TenantName = "Demo Tenant",
                    Type = "Donation",
                    Title = "Support Donation",
                    Amount = 100,
                    Currency = "NZD",
                    Status = "Completed",
                    PaymentStatus = "Succeeded",
                    RiskStatus = "NotChecked",
                    CreatedByUserPublicId = Guid.NewGuid().ToString(),
                    CreatedAtUtc = DateTime.UtcNow
                },
                ResultCodes.Transaction.TransactionQuerySuccess,
                "ok"));

        var sut = new GetTransactionDetailQueryHandler(transactionServiceMock.Object);

        var query = new GetTransactionDetailQuery(
            TenantPublicId: Guid.NewGuid().ToString(),
            UserPublicId: Guid.NewGuid().ToString(),
            Role: "Member",
            TransactionPublicId: Guid.NewGuid().ToString());

        var result = await sut.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();

        transactionServiceMock.Verify(x => x.GetTransactionDetailAsync(
                query.TenantPublicId,
                query.UserPublicId,
                query.Role,
                query.TransactionPublicId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}