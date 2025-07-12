using AutoFixture.Xunit2;
using FluentAssertions;
using IdentityService.CommandHandlers;
using IdentityService.Commands;
using IdentityService.Services.Interfaces;
using IdentityService.Tests.Attributes;
using Moq;
using SharedKernel.Common.Results;

namespace IdentityService.Tests.CommandHandlers;

public class ChangeUserPasswordCommandhandlerTests
{

    [Theory, AutoMoqData]
    public async Task Handle_ShouldReturnResult_WhenRequestArrived(
        [Frozen] Mock<IUserAppService> userAppServiceMock,
        SetUserPasswordCommandHandler sut)
    {
        // Arrange
        var command = new SetUserPasswordCommand(
            Guid.NewGuid().ToString(),
            "1",
            "NewPassword",
            "OldPassword",
            true
        );

    userAppServiceMock.Setup(uas => uas.SetUserPasswordAsync(
            command.UserPublicId, command.JwtVersion, 
            command.OldPassword, command.NewPassword, 
            command.Reset, CancellationToken.None))
            .ReturnsAsync(ServiceResult<bool>.Ok(true, "Change password successfully.", "Change password successfully."));
        
        // Act
        var result = await sut.Handle(command, CancellationToken.None);
        
        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();
    }
}