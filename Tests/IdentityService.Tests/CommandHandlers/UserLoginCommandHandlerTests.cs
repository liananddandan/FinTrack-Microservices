using AutoFixture.Xunit2;
using FluentAssertions;
using IdentityService.CommandHandlers;
using IdentityService.Commands;
using IdentityService.Common.DTOs;
using IdentityService.Services.Interfaces;
using IdentityService.Tests.Attributes;
using Moq;
using SharedKernel.Common.Results;

namespace IdentityService.Tests.CommandHandlers;

public class UserLoginCommandHandlerTests
{
    [Theory, AutoMoqData]
    public async Task Handle_ShouldReturnServiceResult_WhenCommandExecuted(
        [Frozen] Mock<IUserAppService> userAppServiceMock,
        UserLoginCommandHandler sut,
        UserLoginResult userLoginResult)
    {
        // Arrange
        string email = "test@email.com";
        string password = "password";
        var userLoginCommand = new UserLoginCommand(email, password);
        userAppServiceMock.Setup(uas =>
                uas.UserLoginAsync(email, password,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<UserLoginResult>
                .Ok(userLoginResult, "User Login Success", "User Login Success"));

        
        // Act
        var result = await sut.Handle(userLoginCommand, CancellationToken.None);
        
        // Assert
        result.Data.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }
}