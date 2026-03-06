using AutoFixture.Xunit2;
using FluentAssertions;
using IdentityService.CommandHandlers;
using IdentityService.Commands;
using IdentityService.Services.Interfaces;
using IdentityService.Tests.Attributes;
using Moq;
using SharedKernel.Common.Results;

namespace IdentityService.Tests.CommandHandlers;

public class ReceiveInviteCommandHandlerTests
{
    [Theory, AutoMoqData]
    public async Task Handle_ShouldExecute_WhenReceivedCommand(
        [Frozen] Mock<ITenantService> tenantServiceMock,
        ReceiveInviteCommand command,
        ReceiveInviteCommandHandler sut)
    {
        // Arrange
        tenantServiceMock.Setup(
            ts => ts.ReceiveInviteForTenantAsync(command.InvitationPublicId, CancellationToken.None))
            .ReturnsAsync(ServiceResult<bool>.Ok(true, "receive invitation", "receive invitation"));
        
        // Act
        var result = await sut.Handle(command, CancellationToken.None);
        
        // Assert
        result.Success.Should().BeTrue();
    }
}