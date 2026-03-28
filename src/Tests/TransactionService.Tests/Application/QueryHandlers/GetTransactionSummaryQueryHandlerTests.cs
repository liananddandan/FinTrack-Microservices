using FluentAssertions;
using Moq;
using SharedKernel.Common.Results;
using TransactionService.Api.Transaction.Contracts;
using TransactionService.Application.Transactions.Abstractions;
using TransactionService.Application.Transactions.Queries;
using TransactionService.Application.Transactions.QueryHandlers;
using Xunit;

namespace TransactionService.Tests.Application.QueryHandlers;

public class GetTransactionSummaryQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Delegate_To_TransactionService()
    {
        var transactionServiceMock = new Mock<ITransactionService>();

        transactionServiceMock
            .Setup(x => x.GetTransactionSummaryAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TenantTransactionSummaryDto>.Ok(
                new TenantTransactionSummaryDto(),
                ResultCodes.Transaction.TransactionQuerySuccess,
                "ok"));

        var sut = new GetTransactionSummaryQueryHandler(transactionServiceMock.Object);

        var query = new GetTransactionSummaryQuery(
            TenantPublicId: Guid.NewGuid().ToString(),
            Role: "Admin");

        var result = await sut.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();

        transactionServiceMock.Verify(x => x.GetTransactionSummaryAsync(
                query.TenantPublicId,
                query.Role,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}