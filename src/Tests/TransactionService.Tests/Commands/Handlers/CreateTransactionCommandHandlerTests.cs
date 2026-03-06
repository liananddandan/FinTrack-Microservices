using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using SharedKernel.Common.Results;
using TransactionService.Commands;
using TransactionService.Commands.Handlers;
using TransactionService.Common.Responses;
using TransactionService.Services.Interfaces;
using TransactionService.Tests.Attributes;

namespace TransactionService.Tests.Commands.Handlers;

public class CreateTransactionCommandHandlerTests
{
    [Theory, AutoMoqData]
    public async Task Handle_ShouldReturnResult_WhenReceivedCommand(
        [Frozen] Mock<ITransactionService> transactionServiceMock,
        CreateTransactionCommand command,
        CreateTransactionResponse response,
        CreateTransactionCommandHandler sut)
    {
        // arrange
        transactionServiceMock.Setup(ts
                => ts.CreateTransactionAsync(command.TenantPublicId,
                    command.UserPublicId, command.Amount, command.Currency, command.Description))
            .ReturnsAsync(ServiceResult<CreateTransactionResponse>.Ok(response, "created transaction", "created transaction"));
        
        // act
        var result = await sut.Handle(command, CancellationToken.None);
        
        // assert
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(response);
    }
}