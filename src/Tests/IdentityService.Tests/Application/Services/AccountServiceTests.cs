using AutoFixture;
using FluentAssertions;
using IdentityService.Application.Services;
using IdentityService.Application.Services.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using IdentityService.Infrastructure.Persistence.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using SharedKernel.Common.Results;

namespace IdentityService.Tests.Application.Services;

public class AccountServiceTests
{
    private readonly Fixture _fixture = new();

    private readonly Mock<ILogger<AccountService>> _loggerMock = new();
    private readonly Mock<IApplicationUserRepo> _applicationUserRepoMock = new();
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock = new();
    private readonly Mock<IUserDomainService> _userDomainServiceMock = new();

    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;

    private readonly AccountService _sut;

    public AccountServiceTests()
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

        _sut = new AccountService(
            _loggerMock.Object,
            _applicationUserRepoMock.Object,
            _userManagerMock.Object,
            _jwtTokenServiceMock.Object,
            _userDomainServiceMock.Object);
    }

    [Theory]
    [InlineData("", "Password123!", "Email is required.")]
    [InlineData("user@test.com", "", "Password is required.")]
    public async Task LoginAsync_Should_Return_Fail_When_Required_Parameter_Is_Missing(
        string email,
        string password,
        string expectedMessage)
    {
        var result = await _sut.LoginAsync(email, password, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Be(expectedMessage);
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_Should_Return_Fail_When_User_Not_Found()
    {
        _applicationUserRepoMock
            .Setup(x => x.GetUserByEmailWithMembershipsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _sut.LoginAsync("user@test.com", "Password123!", CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task LoginAsync_Should_Return_Fail_When_Password_Is_Invalid()
    {
        var user = new ApplicationUser
        {
            Email = "user@test.com",
            UserName = "user@test.com",
            PublicId = Guid.NewGuid(),
            JwtVersion = 1
        };

        _applicationUserRepoMock
            .Setup(x => x.GetUserByEmailWithMembershipsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, "Password123!"))
            .ReturnsAsync(false);

        var result = await _sut.LoginAsync("user@test.com", "Password123!", CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task LoginAsync_Should_Return_Token_And_Memberships_When_Login_Succeeds()
    {
        var tenant = new Tenant
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            Name = "FinTrack"
        };

        var memberships = new List<TenantMembership>
        {
            new TenantMembership
            {
                Tenant = tenant,
                TenantId = tenant.Id,
                IsActive = true,
                Role = TenantRole.Admin
            }
        };

        var user = new ApplicationUser
        {
            Email = "user@test.com",
            UserName = "user@test.com",
            PublicId = Guid.NewGuid(),
            JwtVersion = 2,
            Memberships = memberships
        };

        _applicationUserRepoMock
            .Setup(x => x.GetUserByEmailWithMembershipsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, "Password123!"))
            .ReturnsAsync(true);

        _jwtTokenServiceMock
            .Setup(x => x.GenerateAccountAccessToken(user))
            .Returns("fake-access-token");

        _jwtTokenServiceMock
            .Setup(x => x.GenerateRefreshToken(user))
            .Returns("fake-refresh-token");

        var result = await _sut.LoginAsync("user@test.com", "Password123!", CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();

        result.Data!.Tokens.AccessToken.Should().Be("fake-access-token");
        result.Data.Tokens.RefreshToken.Should().Be("fake-refresh-token");
        result.Data.Memberships.Should().HaveCount(1);

        var membership = result.Data.Memberships.Single();
        membership.TenantName.Should().Be("FinTrack");
        membership.Role.Should().Be("Admin");
        membership.TenantPublicId.Should().Be(tenant.PublicId.ToString());
    }

    [Fact]
    public async Task LoginAsync_Should_Return_Fail_When_Exception_Is_Thrown()
    {
        _applicationUserRepoMock
            .Setup(x => x.GetUserByEmailWithMembershipsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("db error"));

        var result = await _sut.LoginAsync("user@test.com", "Password123!", CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Login failed.");
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_Return_Fail_When_User_Not_Found()
    {
        _applicationUserRepoMock
            .Setup(x => x.GetUserByPublicIdWithMembershipsAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _sut.RefreshTokenAsync(
            "user-1",
            "1",
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("User not found.");
        result.Code.Should().Be(ResultCodes.Token.RefreshJwtTokenFailedClaimUserNotFound);
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_Return_Fail_When_JwtVersion_Is_Invalid()
    {
        var user = new ApplicationUser
        {
            PublicId = Guid.NewGuid(),
            JwtVersion = 2,
            Memberships = new List<TenantMembership>()
        };

        _applicationUserRepoMock
            .Setup(x => x.GetUserByPublicIdWithMembershipsAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.RefreshTokenAsync(
            "user-1",
            "1",
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid jwt version.");
        result.Code.Should().Be(ResultCodes.Token.RefreshJwtTokenFailedTokenInvalid);
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_Return_New_TokenPair_When_Request_Is_Valid()
    {
        var tenantPublicId = Guid.NewGuid();

        var membership = new TenantMembership
        {
            IsActive = true,
            Role = TenantRole.Admin,
            Tenant = new Tenant
            {
                PublicId = tenantPublicId,
                Name = "FinTrack",
                IsDeleted = false
            }
        };

        var user = new ApplicationUser
        {
            PublicId = Guid.NewGuid(),
            JwtVersion = 1,
            Memberships = new List<TenantMembership> { membership }
        };

        _applicationUserRepoMock
            .Setup(x => x.GetUserByPublicIdWithMembershipsAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _jwtTokenServiceMock
            .Setup(x => x.GenerateAccountAccessToken(user))
            .Returns("new-access-token");

        _jwtTokenServiceMock
            .Setup(x => x.GenerateRefreshToken(user))
            .Returns("new-refresh-token");

        var result = await _sut.RefreshTokenAsync(
            "user-1",
            "1",
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().Be("new-access-token");
        result.Data.RefreshToken.Should().Be("new-refresh-token");

        _userDomainServiceMock.Verify(
            x => x.SyncJwtVersionAsync(user, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_Return_Fail_When_Exception_Is_Thrown()
    {
        _applicationUserRepoMock
            .Setup(x => x.GetUserByPublicIdWithMembershipsAsync("user-1", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("db error"));

        var result = await _sut.RefreshTokenAsync(
            "user-1",
            "1",
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Refresh token failed.");
        result.Code.Should().Be(ResultCodes.Token.RefreshJwtTokenFailedTokenInvalid);
    }

    [Theory]
    [InlineData("", "test@example.com", "Password123!", "User name is required.")]
    [InlineData("Emily", "", "Password123!", "Email is required.")]
    [InlineData("Emily", "test@example.com", "", "Password is required.")]
    public async Task RegisterUserAsync_Should_Return_Fail_When_Required_Parameter_Is_Missing(
        string userName,
        string email,
        string password,
        string expectedMessage)
    {
        var result = await _sut.RegisterUserAsync(userName, email, password, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Be(expectedMessage);
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task RegisterUserAsync_Should_Return_Fail_When_Email_Already_Exists()
    {
        _applicationUserRepoMock
            .Setup(x => x.IsEmailExistsAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.RegisterUserAsync(
            "Emily",
            "test@example.com",
            "Password123!",
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Email already exists.");
    }

    [Fact]
    public async Task RegisterUserAsync_Should_Return_Fail_When_CreateUser_Fails()
    {
        _applicationUserRepoMock
            .Setup(x => x.IsEmailExistsAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "Password123!"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Description = "Password is too weak."
            }));

        var result = await _sut.RegisterUserAsync(
            "Emily",
            "test@example.com",
            "Password123!",
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Password is too weak.");
    }

    [Fact]
    public async Task RegisterUserAsync_Should_Return_Success_When_Request_Is_Valid()
    {
        _applicationUserRepoMock
            .Setup(x => x.IsEmailExistsAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "Password123!"))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.RegisterUserAsync(
            "Emily",
            "test@example.com",
            "Password123!",
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("User registered successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.Email.Should().Be("test@example.com");
        result.Data.UserName.Should().Be("Emily");
        result.Data.UserPublicId.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task RegisterUserAsync_Should_Normalize_Email_To_Lowercase()
    {
        _applicationUserRepoMock
            .Setup(x => x.IsEmailExistsAsync("chenli@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        ApplicationUser? createdUser = null;

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "Password123!"))
            .Callback<ApplicationUser, string>((user, _) => createdUser = user)
            .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.RegisterUserAsync(
            "ChenLi",
            "ChenLi@Example.com",
            "Password123!",
            CancellationToken.None);

        result.Success.Should().BeTrue();
        createdUser.Should().NotBeNull();
        createdUser!.Email.Should().Be("chenli@example.com");
    }

    [Fact]
    public async Task RegisterUserAsync_Should_Return_Fail_When_Exception_Is_Thrown()
    {
        _applicationUserRepoMock
            .Setup(x => x.IsEmailExistsAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("db error"));

        var result = await _sut.RegisterUserAsync(
            "Emily",
            "test@example.com",
            "Password123!",
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("User registration failed.");
    }
[Fact]
    public async Task SelectTenantAsync_Should_Return_Fail_When_UserPublicId_Is_Empty()
    {
        var result = await _sut.SelectTenantAsync(
            "",
            Guid.NewGuid().ToString(),
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("User public id is required.");
    }

    [Fact]
    public async Task SelectTenantAsync_Should_Return_Fail_When_TenantPublicId_Is_Empty()
    {
        var result = await _sut.SelectTenantAsync(
            Guid.NewGuid().ToString(),
            "",
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("Tenant public id is required.");
    }

    [Fact]
    public async Task SelectTenantAsync_Should_Return_Fail_When_User_Not_Found()
    {
        var userPublicId = Guid.NewGuid().ToString();
        var tenantPublicId = Guid.NewGuid().ToString();

        _applicationUserRepoMock
            .Setup(x => x.GetUserByPublicIdWithMembershipsAsync(
                userPublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _sut.SelectTenantAsync(
            userPublicId,
            tenantPublicId,
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("User not found.");
    }

    [Fact]
    public async Task SelectTenantAsync_Should_Return_Fail_When_Membership_Not_Found()
    {
        var userPublicId = Guid.NewGuid().ToString();
        var tenantPublicId = Guid.NewGuid().ToString();

        var user = new ApplicationUser
        {
            Id = 1,
            PublicId = Guid.Parse(userPublicId),
            Email = "user@test.com",
            UserName = "user@test.com",
            JwtVersion = 1,
            Memberships = new List<TenantMembership>()
        };

        _applicationUserRepoMock
            .Setup(x => x.GetUserByPublicIdWithMembershipsAsync(
                userPublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.SelectTenantAsync(
            userPublicId,
            tenantPublicId,
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("Tenant membership not found.");
    }

    [Fact]
    public async Task SelectTenantAsync_Should_Return_Fail_When_Membership_Is_Inactive()
    {
        var userPublicId = Guid.NewGuid().ToString();
        var tenantPublicId = Guid.NewGuid().ToString();

        var membership = new TenantMembership
        {
            TenantId = 10,
            Tenant = new Tenant
            {
                Id = 10,
                PublicId = Guid.Parse(tenantPublicId),
                Name = "FinTrack",
                IsDeleted = false
            },
            UserId = 1,
            Role = TenantRole.Admin,
            IsActive = false
        };

        var user = new ApplicationUser
        {
            Id = 1,
            PublicId = Guid.Parse(userPublicId),
            Email = "user@test.com",
            UserName = "user@test.com",
            JwtVersion = 1,
            Memberships = new List<TenantMembership> { membership }
        };

        _applicationUserRepoMock
            .Setup(x => x.GetUserByPublicIdWithMembershipsAsync(
                userPublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.SelectTenantAsync(
            userPublicId,
            tenantPublicId,
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("Tenant membership not found.");
    }

    [Fact]
    public async Task SelectTenantAsync_Should_Return_Fail_When_Tenant_Is_Deleted()
    {
        var userPublicId = Guid.NewGuid().ToString();
        var tenantPublicId = Guid.NewGuid().ToString();

        var membership = new TenantMembership
        {
            TenantId = 10,
            Tenant = new Tenant
            {
                Id = 10,
                PublicId = Guid.Parse(tenantPublicId),
                Name = "FinTrack",
                IsDeleted = true
            },
            UserId = 1,
            Role = TenantRole.Admin,
            IsActive = true
        };

        var user = new ApplicationUser
        {
            Id = 1,
            PublicId = Guid.Parse(userPublicId),
            Email = "user@test.com",
            UserName = "user@test.com",
            JwtVersion = 1,
            Memberships = new List<TenantMembership> { membership }
        };

        _applicationUserRepoMock
            .Setup(x => x.GetUserByPublicIdWithMembershipsAsync(
                userPublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.SelectTenantAsync(
            userPublicId,
            tenantPublicId,
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("Tenant membership not found.");
    }

    [Fact]
    public async Task SelectTenantAsync_Should_Return_TenantAccessToken_When_Request_Is_Valid()
    {
        var userPublicId = Guid.NewGuid().ToString();
        var tenantPublicId = Guid.NewGuid().ToString();

        var membership = new TenantMembership
        {
            TenantId = 10,
            Tenant = new Tenant
            {
                Id = 10,
                PublicId = Guid.Parse(tenantPublicId),
                Name = "FinTrack",
                IsDeleted = false
            },
            UserId = 1,
            Role = TenantRole.Admin,
            IsActive = true
        };

        var user = new ApplicationUser
        {
            Id = 1,
            PublicId = Guid.Parse(userPublicId),
            Email = "user@test.com",
            UserName = "user@test.com",
            JwtVersion = 1,
            Memberships = new List<TenantMembership> { membership }
        };

        _applicationUserRepoMock
            .Setup(x => x.GetUserByPublicIdWithMembershipsAsync(
                userPublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _jwtTokenServiceMock
            .Setup(x => x.GenerateTenantAccessToken(user, membership))
            .Returns("tenant-access-token");

        var result = await _sut.SelectTenantAsync(
            userPublicId,
            tenantPublicId,
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().Be("tenant-access-token");
        result.Message.Should().Be("Tenant selected successfully.");

        _jwtTokenServiceMock.Verify(
            x => x.GenerateTenantAccessToken(user, membership),
            Times.Once);
    }

    [Fact]
    public async Task SelectTenantAsync_Should_Return_Fail_When_Exception_Is_Thrown()
    {
        var userPublicId = Guid.NewGuid().ToString();
        var tenantPublicId = Guid.NewGuid().ToString();

        _applicationUserRepoMock
            .Setup(x => x.GetUserByPublicIdWithMembershipsAsync(
                userPublicId,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("db error"));

        var result = await _sut.SelectTenantAsync(
            userPublicId,
            tenantPublicId,
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("Failed to select tenant.");
    }
}