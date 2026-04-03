
using IdentityService.Application.Accounts.Abstractions;
using IdentityService.Application.Common.Abstractions;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Platforms.Abstractions;
using IdentityService.Application.Platforms.Dtos;
using IdentityService.Application.Platforms.Services;
using IdentityService.Domain.Entities;
using Moq;
using SharedKernel.Common.Results;
using Xunit;

namespace IdentityService.Tests.UnitTests.Platform;

public class PlatformAccessServiceTests
{
    private readonly Mock<IApplicationUserRepo> _userRepository = new();
    private readonly Mock<IPlatformAccessRepository> _platformAccessRepository = new();
    private readonly Mock<IJwtTokenService> _jwtTokenService = new();

    private readonly PlatformAccessService _service;

    public PlatformAccessServiceTests()
    {
        _service = new PlatformAccessService(
            _userRepository.Object,
            _platformAccessRepository.Object,
            _jwtTokenService.Object);
    }

    [Fact]
    public async Task SelectPlatformAsync_Should_Fail_When_User_Not_Found()
    {
        var userPublicId = Guid.NewGuid().ToString();

        _userRepository
            .Setup(x => x.GetUserByPublicIdAsync(userPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _service.SelectPlatformAsync(userPublicId, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Identity.User.NotFound", result.Code);

        _platformAccessRepository.Verify(x => x.GetByUserPublicIdAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);

        _jwtTokenService.Verify(x => x.GeneratePlatformAccessToken(
            It.IsAny<JwtClaimSource>()), Times.Never);
    }

    [Fact]
    public async Task SelectPlatformAsync_Should_Fail_When_PlatformAccess_Not_Found()
    {
        var userPublicId = Guid.NewGuid().ToString();

        var user = new ApplicationUser
        {
            PublicId = Guid.Parse(userPublicId),
            UserName = "platform-admin",
            Email = "admin@test.com",
            JwtVersion = 1
        };

        _userRepository
            .Setup(x => x.GetUserByPublicIdAsync(userPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _platformAccessRepository
            .Setup(x => x.GetByUserPublicIdAsync(userPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlatformAccess?)null);

        var result = await _service.SelectPlatformAsync(userPublicId, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Identity.PlatformAccess.Forbidden", result.Code);

        _jwtTokenService.Verify(x => x.GeneratePlatformAccessToken(
            It.IsAny<JwtClaimSource>()), Times.Never);
    }

    [Fact]
    public async Task SelectPlatformAsync_Should_Fail_When_PlatformAccess_Is_Disabled()
    {
        var userPublicId = Guid.NewGuid().ToString();

        var user = new ApplicationUser
        {
            PublicId = Guid.Parse(userPublicId),
            UserName = "platform-admin",
            Email = "admin@test.com",
            JwtVersion = 2
        };

        var access = new PlatformAccess
        {
            UserPublicId = userPublicId,
            Role = "SuperAdmin",
            IsEnabled = false
        };

        _userRepository
            .Setup(x => x.GetUserByPublicIdAsync(userPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _platformAccessRepository
            .Setup(x => x.GetByUserPublicIdAsync(userPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(access);

        var result = await _service.SelectPlatformAsync(userPublicId, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Identity.PlatformAccess.Forbidden", result.Code);

        _jwtTokenService.Verify(x => x.GeneratePlatformAccessToken(
            It.IsAny<JwtClaimSource>()), Times.Never);
    }

    [Fact]
    public async Task SelectPlatformAsync_Should_Return_PlatformToken_When_Access_Enabled()
    {
        var userGuid = Guid.NewGuid();
        var userPublicId = userGuid.ToString();

        var user = new ApplicationUser
        {
            PublicId = userGuid,
            UserName = "platform-admin",
            Email = "admin@test.com",
            JwtVersion = 3
        };

        var access = new PlatformAccess
        {
            UserPublicId = userPublicId,
            Role = "SuperAdmin",
            IsEnabled = true
        };

        var tokenDto = new PlatformTokenDto
        {
            PlatformAccessToken = "platform-token",
            PlatformRole = "SuperAdmin"
        };

        _userRepository
            .Setup(x => x.GetUserByPublicIdAsync(userPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _platformAccessRepository
            .Setup(x => x.GetByUserPublicIdAsync(userPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(access);

        _jwtTokenService
            .Setup(x => x.GeneratePlatformAccessToken(
                It.Is<JwtClaimSource>(s =>
                    s.UserPublicId == userPublicId &&
                    s.JwtVersion == "3" &&
                    s.HasPlatformAccess &&
                    s.PlatformRole == "SuperAdmin")))
            .Returns(tokenDto);

        var result = await _service.SelectPlatformAsync(userPublicId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("platform-token", result.Data!.PlatformAccessToken);
        Assert.Equal("SuperAdmin", result.Data.PlatformRole);

        _jwtTokenService.Verify(x => x.GeneratePlatformAccessToken(
            It.Is<JwtClaimSource>(s =>
                s.UserPublicId == userPublicId &&
                s.JwtVersion == "3" &&
                s.HasPlatformAccess &&
                s.PlatformRole == "SuperAdmin")), Times.Once);
    }
}