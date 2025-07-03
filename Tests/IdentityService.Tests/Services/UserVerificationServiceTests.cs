using AutoFixture.Xunit2;
using FluentAssertions;
using IdentityService.Common.Results;
using IdentityService.Common.Status;
using IdentityService.Domain.Entities;
using IdentityService.Services;
using IdentityService.Tests.Attributes;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace IdentityService.Tests.Services;

public class UserVerificationServiceTests
{
    [Theory, AutoMoqData]
    public async Task GenerateTokenAsync_ShouldReturnToken_WhenSuccess(
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        ApplicationUser user,
        UserVerificationService sut)
    {
        // Arrange
        var expectedToken = "mock-token";
        userManagerMock
            .Setup(x
                => x.GenerateUserTokenAsync(user, TokenOptions.DefaultProvider, 
                    TokenPurpose.EmailConfirmation.ToIdentityString()))
            .ReturnsAsync(expectedToken);
        
        // Action
        var result = await sut.GenerateTokenAsync(user, TokenPurpose.EmailConfirmation);
        
        // Assert
        result.Data.Should().NotBeNull();
        result.Data.Should().Be(expectedToken);
        result.Code.Should().BeEquivalentTo(ResultCodes.Token.VerifyTokenGenerateSuccess);
        result.Message.Should().BeEquivalentTo("Verify token generation success.");
        userManagerMock.Verify(userManager => 
            userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultProvider, It.IsAny<string>()), 
            Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task GenerateTokenAsync_ShouldReturnNull_WhenFailed(
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        ApplicationUser user,
        UserVerificationService sut)
    {
        // Arrange
        userManagerMock.Setup(userManager =>
                userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultProvider,
                    TokenPurpose.EmailConfirmation.ToIdentityString()))
            .ReturnsAsync(string.Empty);
        
        // Act
        var result = await sut.GenerateTokenAsync(user, TokenPurpose.EmailConfirmation, CancellationToken.None);
        
        // Assert
        result.Data.Should().BeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.Token.VerifyTokenGenerateFailed);
        result.Message.Should().BeEquivalentTo("Verify token generation failed.");
        userManagerMock.Verify(u => u.GenerateUserTokenAsync(user, TokenOptions.DefaultProvider, It.IsAny<string>()),
            Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task ValidateTokenAsync_ShouldReturnTrue_WhenVerifySuccess(
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        ApplicationUser user,
        string token,
        UserVerificationService sut)
    {
        // Arrange
        userManagerMock.Setup(userManager =>
                userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultProvider,
                    token, It.IsAny<string>()))
            .ReturnsAsync(true);
        
        // Action
        var result = await sut.ValidateTokenAsync(user, token, TokenPurpose.EmailConfirmation, CancellationToken.None);
        
        // Assert
        result.Data.Should().BeTrue();
        result.Code.Should().BeEquivalentTo(ResultCodes.Token.VerifyTokenProcessFinished);
        result.Message.Should().BeEquivalentTo("Verify token process finished.");
    }

    [Theory, AutoMoqData]
    public async Task ValidateTokenAsync_ShouldReturnFalse_WhenVerifyFail(
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        ApplicationUser user,
        string token,
        UserVerificationService sut)
    {
        // Arrange
        userManagerMock.Setup(userManager 
            => userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultProvider, token, It.IsAny<string>()))
            .ReturnsAsync(false);
        
        // Act
        var result = await sut.ValidateTokenAsync(user, token, TokenPurpose.EmailConfirmation, CancellationToken.None);
        
        // Assert
        result.Data.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.Token.VerifyTokenProcessFinished);
        result.Message.Should().BeEquivalentTo("Verify token process finished.");
    }
}