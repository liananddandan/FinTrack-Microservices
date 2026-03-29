using AutoFixture;
using FluentAssertions;
using IdentityService.Application.Accounts.Abstractions;
using IdentityService.Application.Accounts.Services;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using SharedKernel.Common.Results;

namespace IdentityService.Tests.Application.Services;

public class UserAppServiceTests
{
    private readonly Fixture _fixture = new();
    private readonly Mock<ILogger<UserAppService>> _loggerMock = new();
    private readonly Mock<IApplicationUserRepo> _applicationUserRepoMock = new();

    private readonly UserAppService _sut;

    public UserAppServiceTests()
    {
        _sut = new UserAppService(
            _applicationUserRepoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetUserInfoAsync_Should_Return_Fail_When_User_Not_Found()
    {
        var userPublicId = Guid.NewGuid().ToString();

        _applicationUserRepoMock
            .Setup(x => x.GetUserByPublicIdWithMembershipsAsync(userPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _sut.GetUserInfoAsync(userPublicId, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Account.UserNotFound);
        result.Message.Should().Be("User not found.");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetUserInfoAsync_Should_Return_User_Profile_With_Active_Memberships()
    {
        var userPublicId = Guid.NewGuid().ToString();
        var tenant1PublicId = Guid.NewGuid();
        var tenant2PublicId = Guid.NewGuid();

        var activeTenant = new Tenant
        {
            Id = 1,
            PublicId = tenant1PublicId,
            Name = "FinTrack",
            IsDeleted = false
        };

        var inactiveTenant = new Tenant
        {
            Id = 2,
            PublicId = tenant2PublicId,
            Name = "OldTenant",
            IsDeleted = true
        };

        var user = new ApplicationUser
        {
            PublicId = Guid.Parse(userPublicId),
            Email = "emily@test.com",
            UserName = "Emily",
            Memberships = new List<TenantMembership>
            {
                new()
                {
                    Tenant = activeTenant,
                    TenantId = activeTenant.Id,
                    Role = TenantRole.Admin,
                    IsActive = true
                },
                new()
                {
                    Tenant = inactiveTenant,
                    TenantId = inactiveTenant.Id,
                    Role = TenantRole.Member,
                    IsActive = true
                },
                new()
                {
                    Tenant = activeTenant,
                    TenantId = activeTenant.Id,
                    Role = TenantRole.Member,
                    IsActive = false
                }
            }
        };

        _applicationUserRepoMock
            .Setup(x => x.GetUserByPublicIdWithMembershipsAsync(userPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.GetUserInfoAsync(userPublicId, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Code.Should().Be(ResultCodes.Account.GetUserInfoSuccess);
        result.Message.Should().Be("User info fetched successfully.");
        result.Data.Should().NotBeNull();

        result.Data!.UserPublicId.Should().Be(userPublicId);
        result.Data.Email.Should().Be("emily@test.com");
        result.Data.UserName.Should().Be("Emily");
        result.Data.Memberships.Should().HaveCount(1);

        var membership = result.Data.Memberships.Single();
        membership.TenantPublicId.Should().Be(tenant1PublicId.ToString());
        membership.TenantName.Should().Be("FinTrack");
        membership.Role.Should().Be(TenantRole.Admin.ToString());
    }

    [Fact]
    public async Task GetUserInfoAsync_Should_Return_User_Profile_With_Empty_Memberships_When_No_Active_Membership()
    {
        var userPublicId = Guid.NewGuid().ToString();

        var tenant = new Tenant
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            Name = "FinTrack",
            IsDeleted = false
        };

        var user = new ApplicationUser
        {
            PublicId = Guid.Parse(userPublicId),
            Email = "emily@test.com",
            UserName = "Emily",
            Memberships = new List<TenantMembership>
            {
                new()
                {
                    Tenant = tenant,
                    TenantId = tenant.Id,
                    Role = TenantRole.Admin,
                    IsActive = false
                }
            }
        };

        _applicationUserRepoMock
            .Setup(x => x.GetUserByPublicIdWithMembershipsAsync(userPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.GetUserInfoAsync(userPublicId, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Memberships.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserInfoAsync_Should_Return_Fail_When_Exception_Is_Thrown()
    {
        var userPublicId = Guid.NewGuid().ToString();

        _applicationUserRepoMock
            .Setup(x => x.GetUserByPublicIdWithMembershipsAsync(userPublicId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("db error"));

        var result = await _sut.GetUserInfoAsync(userPublicId, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Account.GetUserInfoException);
        result.Message.Should().Be("Failed to get user info.");
        result.Data.Should().BeNull();
    }
}