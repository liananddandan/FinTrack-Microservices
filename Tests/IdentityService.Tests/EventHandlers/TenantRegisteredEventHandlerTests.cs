using AutoFixture.Xunit2;
using DotNetCore.CAP;
using FluentAssertions;
using IdentityService.Common.Results;
using IdentityService.Common.Status;
using IdentityService.Domain.Entities;
using IdentityService.EventHandlers;
using IdentityService.Events;
using IdentityService.Services.Interfaces;
using IdentityService.Tests.Attributes;
using Moq;
using SharedKernel.Common.Exceptions;
using SharedKernel.Events;
using SharedKernel.Topics;

namespace IdentityService.Tests.EventHandlers;

public class TenantRegisteredEventHandlerTests
{
    [Theory, AutoMoqData]
    public async Task Handler_ShouldThrowException_WhenUserNotGetById(
        [Frozen] Mock<IUserAppService> userAppService,
        TenantRegisteredEventHandler sut,
        TenantRegisteredEvent tenantRegisteredEvent)
    {
        // Arrange
        userAppService.Setup(u =>
            u.GetUserByIdAsync(tenantRegisteredEvent.AdminUserId.ToString()))
            .ReturnsAsync(ServiceResult<ApplicationUser>.Fail(ResultCodes.User.UserNotFound, "user not found"));
        
        // Act

        var ex = await FluentActions.Awaiting(() =>
                sut.Handle(tenantRegisteredEvent, CancellationToken.None))
            .Should()
            .ThrowAsync<UserNotFoundException>();
         
       // Assert
       ex.NotBeNull();
       ex.WithMessage("Admin user not found");
    }
    
    [Theory, AutoMoqData]
    public async Task Handler_ShouldThrowException_WhenGenerateTokenFailed(
        [Frozen] Mock<IUserAppService> userAppService,
        [Frozen] Mock<IUserVerificationService> userVerificationService,
        TenantRegisteredEventHandler sut,
        TenantRegisteredEvent tenantRegisteredEvent,
        ApplicationUser user)
    {
        // Arrange
        userAppService.Setup(u =>
                u.GetUserByIdAsync(tenantRegisteredEvent.AdminUserId.ToString()))
            .ReturnsAsync(ServiceResult<ApplicationUser>.Ok(user, ResultCodes.User.UserGetByIdSuccess, "User get Success"));
        
        userVerificationService.Setup(uv => 
            uv.GenerateTokenAsync(user, TokenPurpose.EmailConfirmation, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<string>.Fail(ResultCodes.Token.VerifyTokenGenerateFailed, "token generate failed"));
        // Act

        var ex = await FluentActions.Awaiting(() =>
                sut.Handle(tenantRegisteredEvent, CancellationToken.None))
            .Should()
            .ThrowAsync<TokenGenerateException>();
         
        // Assert
        ex.NotBeNull();
        ex.WithMessage("Generate token failed");
    }
    
    [Theory, AutoMoqData]
    public async Task Handler_ShouldSuccess_WhenEverythingIsOk(
        [Frozen] Mock<IUserAppService> userAppService,
        [Frozen] Mock<IUserVerificationService> userVerificationService,
        [Frozen] Mock<ICapPublisher> capPublisher,
        TenantRegisteredEventHandler sut,
        TenantRegisteredEvent tenantRegisteredEvent,
        ApplicationUser user)
    {
        // Arrange
        userAppService.Setup(u =>
                u.GetUserByIdAsync(tenantRegisteredEvent.AdminUserId.ToString()))
            .ReturnsAsync(ServiceResult<ApplicationUser>.Ok(user, ResultCodes.User.UserGetByIdSuccess, "User get Success"));

        var expectedToken = "expected-token";
        userVerificationService.Setup(uv => 
                uv.GenerateTokenAsync(user, TokenPurpose.EmailConfirmation, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<string>.Ok(expectedToken,ResultCodes.Token.VerifyTokenGenerateSuccess, "token generate Success"));
        
        // Act
        await sut.Handle(tenantRegisteredEvent, CancellationToken.None);
        
        // Assert
        userAppService.Verify(u => 
            u.GetUserByIdAsync(tenantRegisteredEvent.AdminUserId.ToString()), Times.Once);
        userVerificationService.Verify(uv => 
            uv.GenerateTokenAsync(user, TokenPurpose.EmailConfirmation, It.IsAny<CancellationToken>()), Times.Once);
        capPublisher.Verify(c => 
            c.PublishAsync(CapTopics.EmailSend, It.IsAny<EmailSendRequestedEvent>()
            ,new Dictionary<string, string?>(), CancellationToken.None), Times.Once);
    }
}