using AutoFixture.Xunit2;
using FluentAssertions;
using IdentityService.CommandHandlers;
using IdentityService.Commands;
using IdentityService.Services.Interfaces;
using IdentityService.Tests.Attributes;
using Moq;
using SharedKernel.Common.DTOs.Auth;
using SharedKernel.Common.Results;

namespace IdentityService.Tests.CommandHandlers;

public class RefreshUserJwtTokenCommandHandlerTests
{
    [Theory, AutoMoqData]
    public async Task Handle_ShouldReturnResultFromService(
        [Frozen] Mock<IUserAppService> userAppServiceMock,
        RefreshUserJwtTokenCommandHandler sut,
        RefreshUserJwtTokenCommand command,
        JwtTokenPair jwtTokenPair)
    {
        // Arrange
        userAppServiceMock.Setup(uas =>
            uas.RefreshUserTokenPairAsync(command.UserPublicId, command.JwtVersion, CancellationToken.None))
            .ReturnsAsync(ServiceResult<JwtTokenPair>.Ok(jwtTokenPair, "jwt token pair", "jwt token pair"));
        
        // Act
        var result = await sut.Handle(command, CancellationToken.None);
        
        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(jwtTokenPair);
    }
}