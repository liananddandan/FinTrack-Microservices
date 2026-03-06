using AutoFixture.Xunit2;
using DotNetCore.CAP;
using FluentAssertions;
using IdentityService.Common.DTOs;
using IdentityService.Common.Status;
using IdentityService.EventHandlers;
using IdentityService.Events;
using IdentityService.Services.Interfaces;
using IdentityService.Tests.Attributes;
using Moq;
using SharedKernel.Common.Exceptions;
using SharedKernel.Common.Results;
using SharedKernel.Events;
using SharedKernel.Topics;

namespace IdentityService.Tests.EventHandlers;

public class UserFirstLoginEventHandlerTests
{
    [Theory, AutoMoqData]
    public async Task Handle_ShouldFinish_WhenEverythingIsOk(
        [Frozen] Mock<IJwtTokenService> jwtTokenServiceMock,
        [Frozen] Mock<ICapPublisher> capPublisherMock,
        UserFirstLoginEvent userFirstLoginEvent,
        UserFirstLoginEventHandler sut)
    {
        // Arrange
        jwtTokenServiceMock.Setup(jts
                => jts.GenerateJwtTokenAsync(It.IsAny<JwtClaimSource>(), JwtTokenType.FirstLoginToken))
            .ReturnsAsync(ServiceResult<string>.Ok("jwt-user-first-login", "generate jwt first login token",
                "generate jwt first login token"));
        EmailSendRequestedEvent? capturedEmail = null;
        capPublisherMock.Setup(cp
                => cp.PublishAsync(CapTopics.EmailSend,
                    It.IsAny<EmailSendRequestedEvent>(),
                    It.IsAny<IDictionary<string, string?>>(),
                    It.IsAny<CancellationToken>()))
            .Callback<string, EmailSendRequestedEvent, IDictionary<string, string?>, CancellationToken>(
                (type, emailSendRequestedEvent, heads, cancellationToken) =>
                {
                    capturedEmail = emailSendRequestedEvent;
                }
            )
            .Returns(Task.CompletedTask);
        
        // Act
        await FluentActions
            .Awaiting(() => sut.Handle(userFirstLoginEvent, CancellationToken.None))
            .Should()
            .NotThrowAsync();

        // Assert
        capturedEmail.Should().NotBeNull();
        capturedEmail.Body.Should().Contain("jwt-user-first-login");
    }
    
    [Theory, AutoMoqData]
    public async Task Handle_ShouldThrowException_WhenTokenGenerateFail(
        [Frozen] Mock<IJwtTokenService> jwtTokenServiceMock,
        [Frozen] Mock<ICapPublisher> capPublisherMock,
        UserFirstLoginEvent userFirstLoginEvent,
        UserFirstLoginEventHandler sut)
    {
        // Arrange
        jwtTokenServiceMock.Setup(jts
                => jts.GenerateJwtTokenAsync(It.IsAny<JwtClaimSource>(), JwtTokenType.FirstLoginToken))
            .ReturnsAsync(ServiceResult<string>.Fail("Token generate fail", "Token generate fail"));
        EmailSendRequestedEvent? capturedEmail = null;
        capPublisherMock.Setup(cp
                => cp.PublishAsync(CapTopics.EmailSend,
                    It.IsAny<EmailSendRequestedEvent>(),
                    It.IsAny<IDictionary<string, string?>>(),
                    It.IsAny<CancellationToken>()))
            .Callback<string, EmailSendRequestedEvent, IDictionary<string, string?>, CancellationToken>(
                (type, emailSendRequestedEvent, heads, cancellationToken) =>
                {
                    capturedEmail = emailSendRequestedEvent;
                }
            )
            .Returns(Task.CompletedTask);
        
        // Act
        await FluentActions
            .Awaiting(() => sut.Handle(userFirstLoginEvent, CancellationToken.None))
            .Should()
            .ThrowAsync<TokenGenerateException>();

        // Assert
        capturedEmail.Should().BeNull();
    }
}