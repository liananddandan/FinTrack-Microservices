using FluentAssertions;
using IdentityService.Application.Common.Status;
using IdentityService.Application.Events;
using IdentityService.Application.Services;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using IdentityService.Infrastructure.Persistence.Repositories.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace IdentityService.Tests.Application.Services;

public class TenantInvitationServiceTests
{
    private readonly Mock<ITenantRepo> _tenantRepoMock = new();
    private readonly Mock<IApplicationUserRepo> _userRepoMock = new();
    private readonly Mock<ITenantMembershipRepo> _membershipRepoMock = new();
    private readonly Mock<ITenantInvitationRepo> _invitationRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<TenantInvitationService>> _loggerMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();

    private readonly TenantInvitationService _sut;

    public TenantInvitationServiceTests()
    {
        _sut = new TenantInvitationService(
            _tenantRepoMock.Object,
            _userRepoMock.Object,
            _membershipRepoMock.Object,
            _invitationRepoMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _mediatorMock.Object);
    }

    [Fact]
    public async Task CreateInvitationAsync_Should_Return_Fail_When_TenantPublicId_Is_Empty()
    {
        var result = await _sut.CreateInvitationAsync(
            "",
            "user@test.com",
            "Member",
            Guid.NewGuid().ToString(),
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Data.Should().BeFalse();
        result.Message.Should().Be("Tenant public id is required.");
    }

    [Fact]
    public async Task CreateInvitationAsync_Should_Return_Fail_When_Email_Is_Empty()
    {
        var result = await _sut.CreateInvitationAsync(
            Guid.NewGuid().ToString(),
            "",
            "Member",
            Guid.NewGuid().ToString(),
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Data.Should().BeFalse();
        result.Message.Should().Be("Email is required.");
    }

    [Fact]
    public async Task CreateInvitationAsync_Should_Return_Fail_When_Role_Is_Invalid()
    {
        var tenantPublicId = Guid.NewGuid().ToString();
        var inviterPublicId = Guid.NewGuid().ToString();

        _tenantRepoMock
            .Setup(x => x.GetTenantByPublicIdAsync(tenantPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tenant { Id = 1, PublicId = Guid.Parse(tenantPublicId), Name = "FinTrack" });

        _userRepoMock
            .Setup(x => x.GetUserByEmailAsync("user@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationUser
            {
                Id = 10,
                PublicId = Guid.NewGuid(),
                Email = "user@test.com",
                UserName = "user@test.com"
            });

        _membershipRepoMock
            .Setup(x => x.GetMembershipAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantMembership?)null);

        _userRepoMock
            .Setup(x => x.GetUserByPublicIdAsync(inviterPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationUser
            {
                Id = 99,
                PublicId = Guid.Parse(inviterPublicId),
                Email = "admin@test.com",
                UserName = "admin@test.com"
            });

        var result = await _sut.CreateInvitationAsync(
            tenantPublicId,
            "user@test.com",
            "InvalidRole",
            inviterPublicId,
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Data.Should().BeFalse();
        result.Message.Should().Be("Invalid role.");
    }

    [Fact]
    public async Task CreateInvitationAsync_Should_Return_Fail_When_Tenant_Not_Found()
    {
        var tenantPublicId = Guid.NewGuid().ToString();

        _tenantRepoMock
            .Setup(x => x.GetTenantByPublicIdAsync(tenantPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        var result = await _sut.CreateInvitationAsync(
            tenantPublicId,
            "user@test.com",
            "Member",
            Guid.NewGuid().ToString(),
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Data.Should().BeFalse();
        result.Message.Should().Be("Tenant not found.");
    }

    [Fact]
    public async Task CreateInvitationAsync_Should_Return_Fail_When_User_Not_Found()
    {
        var tenantPublicId = Guid.NewGuid().ToString();

        _tenantRepoMock
            .Setup(x => x.GetTenantByPublicIdAsync(tenantPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tenant { Id = 1, PublicId = Guid.Parse(tenantPublicId), Name = "FinTrack" });

        _userRepoMock
            .Setup(x => x.GetUserByEmailAsync("user@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _sut.CreateInvitationAsync(
            tenantPublicId,
            "user@test.com",
            "Member",
            Guid.NewGuid().ToString(),
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Data.Should().BeFalse();
        result.Message.Should().Be("User not found.");
    }

    [Fact]
    public async Task CreateInvitationAsync_Should_Return_Fail_When_User_Already_Belongs_To_Tenant()
    {
        var tenantPublicId = Guid.NewGuid().ToString();
        var user = new ApplicationUser
        {
            Id = 10,
            PublicId = Guid.NewGuid(),
            Email = "user@test.com",
            UserName = "user@test.com"
        };

        _tenantRepoMock
            .Setup(x => x.GetTenantByPublicIdAsync(tenantPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tenant { Id = 1, PublicId = Guid.Parse(tenantPublicId), Name = "FinTrack" });

        _userRepoMock
            .Setup(x => x.GetUserByEmailAsync("user@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _membershipRepoMock
            .Setup(x => x.GetMembershipAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TenantMembership
            {
                TenantId = 1,
                UserId = 10,
                IsActive = true
            });

        var result = await _sut.CreateInvitationAsync(
            tenantPublicId,
            "user@test.com",
            "Member",
            Guid.NewGuid().ToString(),
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Data.Should().BeFalse();
        result.Message.Should().Be("User already belongs to this tenant.");
    }

    [Fact]
    public async Task CreateInvitationAsync_Should_Create_Invitation_And_Publish_Event_When_Request_Is_Valid()
    {
        var tenantPublicId = Guid.NewGuid().ToString();
        var inviterPublicId = Guid.NewGuid().ToString();

        var tenant = new Tenant
        {
            Id = 1,
            PublicId = Guid.Parse(tenantPublicId),
            Name = "FinTrack"
        };

        var invitedUser = new ApplicationUser
        {
            Id = 10,
            PublicId = Guid.NewGuid(),
            Email = "user@test.com",
            UserName = "user@test.com"
        };

        var inviter = new ApplicationUser
        {
            Id = 99,
            PublicId = Guid.Parse(inviterPublicId),
            Email = "admin@test.com",
            UserName = "admin@test.com"
        };

        TenantInvitation? savedInvitation = null;

        _tenantRepoMock
            .Setup(x => x.GetTenantByPublicIdAsync(tenantPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        _userRepoMock
            .Setup(x => x.GetUserByEmailAsync("user@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitedUser);

        _membershipRepoMock
            .Setup(x => x.GetMembershipAsync(tenant.Id, invitedUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantMembership?)null);

        _userRepoMock
            .Setup(x => x.GetUserByPublicIdAsync(inviterPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inviter);

        _invitationRepoMock
            .Setup(x => x.AddAsync(It.IsAny<TenantInvitation>(), It.IsAny<CancellationToken>()))
            .Callback<TenantInvitation, CancellationToken>((inv, _) => savedInvitation = inv)
            .Returns(Task.CompletedTask);

        var result = await _sut.CreateInvitationAsync(
            tenantPublicId,
            "user@test.com",
            "Member",
            inviterPublicId,
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();
        result.Message.Should().Be("Invitation created successfully.");

        savedInvitation.Should().NotBeNull();
        savedInvitation!.Email.Should().Be("user@test.com");
        savedInvitation.TenantId.Should().Be(tenant.Id);
        savedInvitation.Role.Should().Be(TenantRole.Member);
        savedInvitation.Status.Should().Be(InvitationStatus.Pending);
        savedInvitation.CreatedByUserId.Should().Be(inviter.Id);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        _mediatorMock.Verify(
            x => x.Publish(It.IsAny<TenantInvitationCreatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}