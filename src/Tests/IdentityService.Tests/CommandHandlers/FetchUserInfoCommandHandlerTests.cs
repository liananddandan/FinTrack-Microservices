using AutoFixture.Xunit2;
using FluentAssertions;
using IdentityService.Application.CommandHandlers;
using IdentityService.Application.Commands;
using IdentityService.Application.Services.Interfaces;
using IdentityService.Tests.Attributes;
using Moq;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;

namespace IdentityService.Tests.CommandHandlers;

public class FetchUserInfoCommandHandlerTests
{
    [Theory, AutoMoqData]
    public async Task Handle_ShouldReturnResult_WhenRecievedCommand(
        [Frozen] Mock<IUserAppService> userAppServiceMock,
        FetchUserInfoCommand require,
        UserInfoDto userInfo,
        FetchUserInfoCommandHandler sut)
    {
        // Arrange
        userAppServiceMock.Setup(uas 
            => uas.GetUserInfoAsync(require.UserPublicId, CancellationToken.None))
            .ReturnsAsync(ServiceResult<UserInfoDto>.Ok(userInfo, "get user info", "get user info"));
        
        // Act
        var result = await sut.Handle(require, CancellationToken.None);
        
        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(userInfo);
    }
}