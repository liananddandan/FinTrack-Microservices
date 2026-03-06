using AutoFixture;
using FluentAssertions;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Services;
using IdentityService.Application.Services.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using IdentityService.Infrastructure.Persistence.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace IdentityService.Tests.Services;

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
    public async Task LoginAsync_Should_Return_Fail_When_User_Has_No_Active_Membership()
    {
        var tenant = new Tenant
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            Name = "FinTrack"
        };

        var user = new ApplicationUser
        {
            Email = "user@test.com",
            UserName = "user@test.com",
            PublicId = Guid.NewGuid(),
            JwtVersion = 1,
            Memberships = new List<TenantMembership>
            {
                new TenantMembership
                {
                    Tenant = tenant,
                    TenantId = tenant.Id,
                    IsActive = false,
                    Role = TenantRole.Member
                }
            }
        };

        _applicationUserRepoMock
            .Setup(x => x.GetUserByEmailWithMembershipsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, "Password123!"))
            .ReturnsAsync(true);

        var result = await _sut.LoginAsync("user@test.com", "Password123!", CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("User does not belong to any tenant.");
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
            .Setup(x => x.GenerateAccessToken(user, It.IsAny<IEnumerable<TenantMembership>>()))
            .Returns("fake-access-token");

        _jwtTokenServiceMock
            .Setup(x => x.GenerateRefreshToken(user))
            .Returns("fake-refresh-token");

        var result = await _sut.LoginAsync("user@test.com", "Password123!", CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();

        result.Data!.AccessToken.Should().Be("fake-access-token");
        result.Data.RefreshToken.Should().Be("fake-refresh-token");
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
}