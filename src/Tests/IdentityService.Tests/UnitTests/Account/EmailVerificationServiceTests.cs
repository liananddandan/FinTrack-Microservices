using FluentAssertions;
using IdentityService.Application.Accounts.Abstractions;
using IdentityService.Application.Accounts.Services;
using IdentityService.Application.Common.Abstractions;
using IdentityService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace IdentityService.Tests.UnitTests.Account;

public class EmailVerificationServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IEmailVerificationTokenRepo> _tokenRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    private readonly EmailVerificationService _service;

    public EmailVerificationServiceTests()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();

        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);

        _tokenRepoMock = new Mock<IEmailVerificationTokenRepo>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _service = new EmailVerificationService(
            _userManagerMock.Object,
            _tokenRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task CreateTokenAsync_WhenUserDoesNotExist_ReturnsFail()
    {
        // Arrange
        _userManagerMock
            .Setup(x => x.FindByIdAsync("123"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _service.CreateTokenAsync(123);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("EMAIL_VERIFICATION_USER_NOT_FOUND", result.Code);

        _tokenRepoMock.Verify(
            x => x.GetActiveTokensByUserIdAsync(It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _tokenRepoMock.Verify(
            x => x.AddAsync(It.IsAny<EmailVerificationToken>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateTokenAsync_WhenEmailAlreadyConfirmed_ReturnsFail()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = 123,
            Email = "test@test.com",
            UserName = "tester",
            EmailConfirmed = true
        };

        _userManagerMock
            .Setup(x => x.FindByIdAsync("123"))
            .ReturnsAsync(user);

        // Act
        var result = await _service.CreateTokenAsync(123);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("EMAIL_VERIFICATION_ALREADY_CONFIRMED", result.Code);

        _tokenRepoMock.Verify(
            x => x.GetActiveTokensByUserIdAsync(It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _tokenRepoMock.Verify(
            x => x.AddAsync(It.IsAny<EmailVerificationToken>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateTokenAsync_WhenUserExistsAndEmailNotConfirmed_CreatesTokenAndSaves()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = 123,
            Email = "test@test.com",
            UserName = "tester",
            EmailConfirmed = false
        };

        var oldToken1 = new EmailVerificationToken
        {
            Id = 1,
            UserId = 123,
            TokenHash = "OLD1",
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            ExpiresAt = DateTime.UtcNow.AddHours(23)
        };

        var oldToken2 = new EmailVerificationToken
        {
            Id = 2,
            UserId = 123,
            TokenHash = "OLD2",
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            ExpiresAt = DateTime.UtcNow.AddHours(22)
        };

        var activeTokens = new List<EmailVerificationToken> { oldToken1, oldToken2 };

        _userManagerMock
            .Setup(x => x.FindByIdAsync("123"))
            .ReturnsAsync(user);

        _tokenRepoMock
            .Setup(x => x.GetActiveTokensByUserIdAsync(
                123,
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeTokens);

        EmailVerificationToken? addedToken = null;

        _tokenRepoMock
            .Setup(x => x.AddAsync(It.IsAny<EmailVerificationToken>(), It.IsAny<CancellationToken>()))
            .Callback<EmailVerificationToken, CancellationToken>((token, _) => addedToken = token)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateTokenAsync(123, "127.0.0.1");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("EMAIL_VERIFICATION_TOKEN_CREATED", result.Code);
        Assert.NotNull(result.Data);
        Assert.False(string.IsNullOrWhiteSpace(result.Data!.RawToken));

        Assert.NotNull(addedToken);
        Assert.Equal(123, addedToken!.UserId);
        Assert.False(string.IsNullOrWhiteSpace(addedToken.TokenHash));
        Assert.Equal("127.0.0.1", addedToken.CreatedByIp);
        Assert.True(addedToken.ExpiresAt > addedToken.CreatedAt);

        Assert.NotNull(oldToken1.RevokedAt);
        Assert.NotNull(oldToken2.RevokedAt);

        _tokenRepoMock.Verify(
            x => x.GetActiveTokensByUserIdAsync(123, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _tokenRepoMock.Verify(
            x => x.AddAsync(It.IsAny<EmailVerificationToken>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateTokenAsync_WhenNoActiveTokens_StillCreatesNewToken()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = 456,
            Email = "new@test.com",
            UserName = "newuser",
            EmailConfirmed = false
        };

        _userManagerMock
            .Setup(x => x.FindByIdAsync("456"))
            .ReturnsAsync(user);

        _tokenRepoMock
            .Setup(x => x.GetActiveTokensByUserIdAsync(
                456,
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EmailVerificationToken>());

        EmailVerificationToken? addedToken = null;

        _tokenRepoMock
            .Setup(x => x.AddAsync(It.IsAny<EmailVerificationToken>(), It.IsAny<CancellationToken>()))
            .Callback<EmailVerificationToken, CancellationToken>((token, _) => addedToken = token)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateTokenAsync(456);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotNull(addedToken);
        Assert.Equal(456, addedToken!.UserId);

        _tokenRepoMock.Verify(
            x => x.AddAsync(It.IsAny<EmailVerificationToken>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [Fact]
    public async Task VerifyTokenAsync_Should_Return_Fail_When_Token_Is_Empty()
    {
        var result = await _service.VerifyTokenAsync("");

        result.Success.Should().BeFalse();
        result.Code.Should().Be("EMAIL_VERIFICATION_TOKEN_REQUIRED");
    }

    [Fact]
    public async Task VerifyTokenAsync_Should_Return_Fail_When_Token_Not_Found()
    {
        _tokenRepoMock
            .Setup(x => x.GetByTokenHashWithUserAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailVerificationToken?)null);

        var result = await _service.VerifyTokenAsync("raw-token");

        result.Success.Should().BeFalse();
        result.Code.Should().Be("EMAIL_VERIFICATION_TOKEN_INVALID");
    }

    [Fact]
    public async Task VerifyTokenAsync_Should_Return_Success_When_Token_Already_Used()
    {
        var token = new EmailVerificationToken
        {
            UsedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = new ApplicationUser
            {
                EmailConfirmed = true
            }
        };

        _tokenRepoMock
            .Setup(x => x.GetByTokenHashWithUserAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        var result = await _service.VerifyTokenAsync("raw-token");

        result.Success.Should().BeTrue();
        result.Code.Should().Be("EMAIL_ALREADY_VERIFIED");
        result.Message.Should().Be("Email is already verified.");
    }

    [Fact]
    public async Task VerifyTokenAsync_Should_Return_Fail_When_Token_Is_Revoked()
    {
        var token = new EmailVerificationToken
        {
            RevokedAt = DateTime.UtcNow,
            User = new ApplicationUser()
        };

        _tokenRepoMock
            .Setup(x => x.GetByTokenHashWithUserAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        var result = await _service.VerifyTokenAsync("raw-token");

        result.Success.Should().BeFalse();
        result.Code.Should().Be("EMAIL_VERIFICATION_TOKEN_REVOKED");
    }

    [Fact]
    public async Task VerifyTokenAsync_Should_Return_Fail_When_Token_Is_Expired()
    {
        var token = new EmailVerificationToken
        {
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
            User = new ApplicationUser()
        };

        _tokenRepoMock
            .Setup(x => x.GetByTokenHashWithUserAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        var result = await _service.VerifyTokenAsync("raw-token");

        result.Success.Should().BeFalse();
        result.Code.Should().Be("EMAIL_VERIFICATION_TOKEN_EXPIRED");
    }

    [Fact]
    public async Task VerifyTokenAsync_Should_Return_Fail_When_User_Not_Found()
    {
        var token = new EmailVerificationToken
        {
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = null
        };

        _tokenRepoMock
            .Setup(x => x.GetByTokenHashWithUserAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        var result = await _service.VerifyTokenAsync("raw-token");

        result.Success.Should().BeFalse();
        result.Code.Should().Be("EMAIL_VERIFICATION_USER_NOT_FOUND");
    }

    [Fact]
    public async Task VerifyTokenAsync_Should_Return_Success_When_User_Already_Verified()
    {
        var user = new ApplicationUser
        {
            EmailConfirmed = true
        };

        var token = new EmailVerificationToken
        {
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = user
        };

        _tokenRepoMock
            .Setup(x => x.GetByTokenHashWithUserAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.VerifyTokenAsync("raw-token");

        result.Success.Should().BeTrue();
        result.Code.Should().Be("EMAIL_ALREADY_VERIFIED");
        token.UsedAt.Should().NotBeNull();

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task VerifyTokenAsync_Should_Verify_User_And_Save_When_Token_Is_Valid()
    {
        var user = new ApplicationUser
        {
            EmailConfirmed = false
        };

        var token = new EmailVerificationToken
        {
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = user
        };

        _tokenRepoMock
            .Setup(x => x.GetByTokenHashWithUserAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.VerifyTokenAsync("raw-token");

        result.Success.Should().BeTrue();
        result.Code.Should().Be("EMAIL_VERIFICATION_SUCCESS");

        user.EmailConfirmed.Should().BeTrue();
        token.UsedAt.Should().NotBeNull();

        _userManagerMock.Verify(
            x => x.UpdateAsync(It.Is<ApplicationUser>(u => u.EmailConfirmed)),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}