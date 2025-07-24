using AutoFixture.Xunit2;
using FluentAssertions;
using IdentityService.Common.Status;
using IdentityService.Domain.Entities;
using IdentityService.Services;
using IdentityService.Tests.Attributes;
using Microsoft.AspNetCore.Identity;
using Moq;
using SharedKernel.Common.Results;

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
                => x.GenerateEmailConfirmationTokenAsync(user))
            .ReturnsAsync(expectedToken);
        
        // Action
        var result = await sut.GenerateTokenAsync(user, TokenPurpose.EmailConfirmation);
        
        // Assert
        result.Data.Should().NotBeNull();
        result.Data.Should().Be(expectedToken);
        result.Code.Should().BeEquivalentTo(ResultCodes.Token.VerifyTokenGenerateSuccess);
        result.Message.Should().BeEquivalentTo("Verify token generation success.");
        userManagerMock.Verify(userManager => 
            userManager.GenerateEmailConfirmationTokenAsync(user), 
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
                userManager.GenerateEmailConfirmationTokenAsync(user))
            .ReturnsAsync(string.Empty);
        
        // Act
        var result = await sut.GenerateTokenAsync(user, TokenPurpose.EmailConfirmation, CancellationToken.None);
        
        // Assert
        result.Data.Should().BeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.Token.VerifyTokenGenerateFailed);
        result.Message.Should().BeEquivalentTo("Verify token generation failed.");
        userManagerMock.Verify(u
                => u.GenerateEmailConfirmationTokenAsync(user),
            Times.Once);
    }
    
    [Theory, AutoMoqData]
    public async Task GenerateTokenAsync_ShouldReturnNull_WhenTypeInvalid(
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        ApplicationUser user,
        UserVerificationService sut)
    {
        // Arrange
        
        // Act
        var result = await sut.GenerateTokenAsync(user, TokenPurpose.InvalidType, CancellationToken.None);
        
        // Assert
        result.Data.Should().BeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.Token.VerifyTokenGenerateFailed);
        result.Message.Should().BeEquivalentTo("Verify token generation failed.");
        userManagerMock.Verify(u
                => u.GenerateEmailConfirmationTokenAsync(user),
            Times.Never);
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
                userManager.ConfirmEmailAsync(user, token))
            .ReturnsAsync(IdentityResult.Success);
        
        // Action
        var result = await sut.ValidateTokenAsync(user, token, TokenPurpose.EmailConfirmation, CancellationToken.None);
        
        // Assert
        result.Data.Should().BeTrue();
        result.Code.Should().BeEquivalentTo(ResultCodes.Token.VerifyTokenSuccess);
        result.Message.Should().BeEquivalentTo("Token Verification Successful.");
    }

    [Theory, AutoMoqData]
    public async Task ValidateTokenAsync_ShouldReturnFalse_WhenVerifyFail(
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        ApplicationUser user,
        string token,
        UserVerificationService sut,
        IdentityError identityError)
    {
        // Arrange
        userManagerMock.Setup(userManager 
            => userManager.ConfirmEmailAsync(user, token))
            .ReturnsAsync(IdentityResult.Failed(identityError));
        
        // Act
        var result = await sut.ValidateTokenAsync(user, token, TokenPurpose.EmailConfirmation, CancellationToken.None);
        
        // Assert
        result.Data.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.Token.VerifyTokenFailed);
    }
    
    [Theory, AutoMoqData]
    public async Task ValidateTokenAsync_ShouldReturnFail_WhenTypeInvalid(
        ApplicationUser user,
        string token,
        UserVerificationService sut)
    {
        // Arrange
        
        // Act
        var result = await sut.ValidateTokenAsync(user, token, TokenPurpose.InvalidType, CancellationToken.None);
        
        // Assert
        result.Data.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.Token.VerifyTokenGenerateInvalidTokenType);
        result.Message.Should().BeEquivalentTo("Token Type is not supported.");
    }
    
}