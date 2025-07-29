using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using SharedKernel.Common.Results;
using TransactionService.Commands;
using TransactionService.Commands.Handlers;
using TransactionService.Common.DTOs;
using TransactionService.Services.Interfaces;
using TransactionService.Tests.Attributes;

namespace TransactionService.Tests.Commands.Handlers;

public class QueryTransactionByPageCommandHandlerTests
{
    [Theory, AutoMoqData]
    public async Task Handle_ShouldReturnResult_WhenReceiveCommand(
        [Frozen] Mock<ITransactionService> serviceMock,
        QueryTransactionByPageCommand command,
        QueryByPageDto queryByPageDto,
        QueryTransactionByPageCommandHandler sut)
    {
        // arrange
        serviceMock.Setup(ts => ts.QueryTransactionByPageAsync(command.TenantPublicId,
            command.UserPublicId, command.StartDate, command.EndDate, command.Page,
            command.PageSize, command.SortBy))
            .ReturnsAsync(ServiceResult<QueryByPageDto>
                .Ok(queryByPageDto, "query success", "query success"));
        
        // act
        var result = await sut.Handle(command, CancellationToken.None);
        
        // assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEquivalentTo(queryByPageDto);
    }
}