using FluentAssertions;
using Moq;
using SharedKernel.Common.Results;
using TransactionService.Application.Common.DTOs;
using TransactionService.Application.Queries;
using TransactionService.Application.QueryHandlers;
using TransactionService.Application.Services.Interfaces;
using Xunit;

namespace TransactionService.Tests.Application.QueryHandlers;

public class GetMyTransactionsQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Delegate_To_TransactionService()
    {
        var transactionServiceMock = new Mock<ITransactionService>();

        transactionServiceMock
            .Setup(x => x.GetMyTransactionsAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<PagedResult<TransactionListItemDto>>.Ok(
                new PagedResult<TransactionListItemDto>
                {
                    Items =
                    [
                        new TransactionListItemDto
                        {
                            TransactionPublicId = Guid.NewGuid().ToString(),
                            TenantPublicId = Guid.NewGuid().ToString(),
                            TenantName = "Demo School",
                            Type = "Donation",
                            Title = "Support Donation",
                            Amount = 100,
                            Currency = "NZD",
                            Status = "Completed",
                            PaymentStatus = "Succeeded",
                            RiskStatus = "NotChecked",
                            CreatedAtUtc = DateTime.UtcNow
                        }
                    ],
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                },
                ResultCodes.Transaction.TransactionQueryByPageSuccess,
                "ok"));

        var sut = new GetMyTransactionsQueryHandler(transactionServiceMock.Object);

        var query = new GetMyTransactionsQuery(
            TenantPublicId: Guid.NewGuid().ToString(),
            UserPublicId: Guid.NewGuid().ToString(),
            PageNumber: 1,
            PageSize: 10);

        var result = await sut.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(1);

        transactionServiceMock.Verify(x => x.GetMyTransactionsAsync(
                query.TenantPublicId,
                query.UserPublicId,
                query.PageNumber,
                query.PageSize,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}