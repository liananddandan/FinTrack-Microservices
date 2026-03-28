using FluentAssertions;
using Moq;
using SharedKernel.Common.Results;
using TransactionService.Application.Abstractions;
using TransactionService.Application.CommandHandlers;
using TransactionService.Application.Commands;
using TransactionService.Application.Common.DTOs;
using Xunit;

namespace TransactionService.Tests.Application.CommandHandlers;

public class CreateDonationCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Delegate_To_TransactionService()
    {
        var transactionServiceMock = new Mock<ITransactionService>();

        transactionServiceMock
            .Setup(x => x.CreateDonationAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<decimal>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
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
                    PaymentStatus = "Succeeded"
                },
                ResultCodes.Transaction.TransactionCreateSuccess,
                "ok"));

        var sut = new CreateDonationCommandHandler(transactionServiceMock.Object);

        var command = new CreateDonationCommand(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            "Donation",
            "Monthly support",
            100,
            "NZD");

        var result = await sut.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();

        transactionServiceMock.Verify(x => x.CreateDonationAsync(
                command.TenantPublicId,
                command.CreatedByUserPublicId,
                command.Title,
                command.Description,
                command.Amount,
                command.Currency,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}