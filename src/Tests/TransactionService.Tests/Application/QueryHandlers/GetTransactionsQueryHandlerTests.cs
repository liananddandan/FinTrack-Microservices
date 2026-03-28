using FluentAssertions;
using Moq;
using SharedKernel.Common.Results;
using TransactionService.Application.Abstractions;
using TransactionService.Application.Common.DTOs;
using TransactionService.Application.Queries;
using TransactionService.Application.QueryHandlers;
using Xunit;

namespace TransactionService.Tests.Application.QueryHandlers;

public class GetTransactionsQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Delegate_To_TransactionService()
    {
        var transactionServiceMock = new Mock<ITransactionService>();

        transactionServiceMock
            .Setup(x => x.GetTransactionsAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<PagedResult<TransactionListItemDto>>.Ok(
                new PagedResult<TransactionListItemDto>(),
                ResultCodes.Transaction.TransactionQueryByPageSuccess,
                "ok"));

        var sut = new GetTransactionsQueryHandler(transactionServiceMock.Object);

        var query = new GetTransactionsQuery(
            TenantPublicId: Guid.NewGuid().ToString(),
            Role: "Admin",
            Type: null,
            Status: null,
            PaymentStatus: null,
            PageNumber: 1,
            PageSize: 10);

        var result = await sut.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();

        transactionServiceMock.Verify(x => x.GetTransactionsAsync(
                query.TenantPublicId,
                query.Role,
                query.Type,
                query.Status,
                query.PaymentStatus,
                query.PageNumber,
                query.PageSize,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}