using FluentAssertions;
using IdentityService.Application.Accounts.Abstractions;
using IdentityService.Application.Accounts.Commands;
using IdentityService.Application.Accounts.Dtos;
using IdentityService.Application.Accounts.Events;
using IdentityService.Application.Accounts.Handlers;
using IdentityService.Domain.Entities;
using MediatR;
using Moq;
using SharedKernel.Common.Results;

namespace IdentityService.Tests.UnitTests.Account;


public class ResendVerificationEmailCommandHandlerTests
{
    private readonly Mock<IApplicationUserRepo> _applicationUserRepoMock = new();
    private readonly Mock<IEmailVerificationService> _emailVerificationServiceMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();

    private readonly ResendVerificationEmailCommandHandler _sut;

    public ResendVerificationEmailCommandHandlerTests()
    {
        _sut = new ResendVerificationEmailCommandHandler(
            _applicationUserRepoMock.Object,
            _emailVerificationServiceMock.Object,
            _mediatorMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Fail_When_User_Not_Found()
    {
        _applicationUserRepoMock
            .Setup(x => x.GetUserByPublicIdAsync("user-public-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _sut.Handle(
            new ResendVerificationEmailCommand("user-public-id"),
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("EMAIL_VERIFICATION_USER_NOT_FOUND");
        result.Message.Should().Be("User not found.");

        _emailVerificationServiceMock.Verify(
            x => x.CreateTokenAsync(It.IsAny<long>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _mediatorMock.Verify(
            x => x.Publish(It.IsAny<SendEmailVerificationRequestedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Fail_When_Email_Already_Confirmed()
    {
        var user = new ApplicationUser
        {
            Id = 1,
            Email = "confirmed@test.com",
            UserName = "confirmed-user",
            EmailConfirmed = true
        };

        _applicationUserRepoMock
            .Setup(x => x.GetUserByPublicIdAsync("user-public-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.Handle(
            new ResendVerificationEmailCommand("user-public-id"),
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("EMAIL_VERIFICATION_ALREADY_CONFIRMED");
        result.Message.Should().Be("Email is already verified.");

        _emailVerificationServiceMock.Verify(
            x => x.CreateTokenAsync(It.IsAny<long>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _mediatorMock.Verify(
            x => x.Publish(It.IsAny<SendEmailVerificationRequestedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Fail_When_Token_Creation_Fails()
    {
        var user = new ApplicationUser
        {
            Id = 1,
            Email = "test@test.com",
            UserName = "test-user",
            EmailConfirmed = false
        };

        _applicationUserRepoMock
            .Setup(x => x.GetUserByPublicIdAsync("user-public-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _emailVerificationServiceMock
            .Setup(x => x.CreateTokenAsync(user.Id, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<CreateEmailVerificationTokenResult>.Fail(
                "EMAIL_VERIFICATION_TOKEN_CREATE_FAILED",
                "Failed to create email verification token."));

        var result = await _sut.Handle(
            new ResendVerificationEmailCommand("user-public-id"),
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("EMAIL_VERIFICATION_TOKEN_CREATE_FAILED");

        _mediatorMock.Verify(
            x => x.Publish(It.IsAny<SendEmailVerificationRequestedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Success_And_Publish_Event_When_Request_Is_Valid()
    {
        var user = new ApplicationUser
        {
            Id = 1,
            Email = "test@test.com",
            UserName = "test-user",
            EmailConfirmed = false
        };

        _applicationUserRepoMock
            .Setup(x => x.GetUserByPublicIdAsync("user-public-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _emailVerificationServiceMock
            .Setup(x => x.CreateTokenAsync(user.Id, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<CreateEmailVerificationTokenResult>.Ok(
                new CreateEmailVerificationTokenResult("raw-token", DateTime.UtcNow.AddHours(24)),
                "EMAIL_VERIFICATION_TOKEN_CREATED",
                "Email verification token created successfully."));

        var result = await _sut.Handle(
            new ResendVerificationEmailCommand("user-public-id"),
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Code.Should().Be("EMAIL_VERIFICATION_RESENT");
        result.Data.Should().BeTrue();

        _mediatorMock.Verify(
            x => x.Publish(
                It.Is<SendEmailVerificationRequestedEvent>(e =>
                    e.UserId == user.Id &&
                    e.Email == user.Email &&
                    e.UserName == user.UserName &&
                    e.EmailVerificationRawToken == "raw-token"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}