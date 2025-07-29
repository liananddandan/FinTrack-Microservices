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

public class QueryTransactionCommandHandlerTests
{
    [Theory, AutoMoqData]
    public async Task Handle_ShouldReturnResult_WhenReceivedCommand(
        [Frozen] Mock<ITransactionService> transactionServiceMock,
        QueryTransactionCommand command,
        TransactionDto transactionDto,
        QueryTransactionCommandHandler sut)
    {
        transactionServiceMock.Setup(ts => ts.QueryUserOwnTransactionByPublicIdAsync(
            command.TenantPublicId, command.UserPublicId, command.TransactionPublicId))
            .ReturnsAsync(ServiceResult<TransactionDto>.Ok(transactionDto, "query success", "query success"));
        
        // act
        var result = await sut.Handle(command, CancellationToken.None);
        
        // assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEquivalentTo(transactionDto);
    }
    
}