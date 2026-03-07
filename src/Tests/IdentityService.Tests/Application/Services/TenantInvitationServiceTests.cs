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
    
    [Fact]
    public async Task ResolveInvitationAsync_Should_Return_Fail_When_Invitation_Not_Found()
    {
        _invitationRepoMock
            .Setup(x => x.GetByPublicIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantInvitation?)null);

        var result = await _sut.ResolveInvitationAsync(
            Guid.NewGuid().ToString(),
            "1",
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invitation not found.");
    }

    [Fact]
    public async Task ResolveInvitationAsync_Should_Return_Fail_When_Version_Is_Invalid()
    {
        var invitation = BuildPendingInvitation(version: 2);

        _invitationRepoMock
            .Setup(x => x.GetByPublicIdAsync(invitation.PublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        var result = await _sut.ResolveInvitationAsync(
            invitation.PublicId.ToString(),
            "1",
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invitation version is invalid.");
    }

    [Fact]
    public async Task ResolveInvitationAsync_Should_Return_Fail_When_Status_Is_Not_Pending()
    {
        var invitation = BuildPendingInvitation();
        invitation.Status = InvitationStatus.Accepted;

        _invitationRepoMock
            .Setup(x => x.GetByPublicIdAsync(invitation.PublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        var result = await _sut.ResolveInvitationAsync(
            invitation.PublicId.ToString(),
            invitation.Version.ToString(),
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invitation is no longer available.");
    }

    [Fact]
    public async Task ResolveInvitationAsync_Should_Return_Fail_When_Invitation_Is_Expired()
    {
        var invitation = BuildPendingInvitation();
        invitation.ExpiredAt = DateTime.UtcNow.AddMinutes(-1);

        _invitationRepoMock
            .Setup(x => x.GetByPublicIdAsync(invitation.PublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        var result = await _sut.ResolveInvitationAsync(
            invitation.PublicId.ToString(),
            invitation.Version.ToString(),
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invitation has expired.");
    }

    [Fact]
    public async Task ResolveInvitationAsync_Should_Return_Invitation_Details_When_Request_Is_Valid()
    {
        var invitation = BuildPendingInvitation();

        _invitationRepoMock
            .Setup(x => x.GetByPublicIdAsync(invitation.PublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        var result = await _sut.ResolveInvitationAsync(
            invitation.PublicId.ToString(),
            invitation.Version.ToString(),
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.InvitationPublicId.Should().Be(invitation.PublicId.ToString());
        result.Data.TenantName.Should().Be(invitation.Tenant.Name);
        result.Data.Email.Should().Be(invitation.Email);
        result.Data.Role.Should().Be(invitation.Role.ToString());
        result.Data.Status.Should().Be(invitation.Status.ToString());
    }

    [Fact]
    public async Task AcceptInvitationAsync_Should_Return_Fail_When_Invitation_Not_Found()
    {
        _invitationRepoMock
            .Setup(x => x.GetByPublicIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantInvitation?)null);

        var result = await _sut.AcceptInvitationAsync(
            Guid.NewGuid().ToString(),
            "1",
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invitation not found.");
    }

    [Fact]
    public async Task AcceptInvitationAsync_Should_Return_Fail_When_Version_Is_Invalid()
    {
        var invitation = BuildPendingInvitation(version: 2);

        _invitationRepoMock
            .Setup(x => x.GetByPublicIdAsync(invitation.PublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        var result = await _sut.AcceptInvitationAsync(
            invitation.PublicId.ToString(),
            "1",
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invitation version is invalid.");
    }

    [Fact]
    public async Task AcceptInvitationAsync_Should_Return_Fail_When_Invitation_Has_Expired()
    {
        var invitation = BuildPendingInvitation();
        invitation.ExpiredAt = DateTime.UtcNow.AddMinutes(-5);

        _invitationRepoMock
            .Setup(x => x.GetByPublicIdAsync(invitation.PublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        var result = await _sut.AcceptInvitationAsync(
            invitation.PublicId.ToString(),
            invitation.Version.ToString(),
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invitation has expired.");
    }

    [Fact]
    public async Task AcceptInvitationAsync_Should_Return_Fail_When_User_Not_Found()
    {
        var invitation = BuildPendingInvitation();

        _invitationRepoMock
            .Setup(x => x.GetByPublicIdAsync(invitation.PublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        _userRepoMock
            .Setup(x => x.GetUserByEmailAsync(invitation.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _sut.AcceptInvitationAsync(
            invitation.PublicId.ToString(),
            invitation.Version.ToString(),
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invited user not found.");
    }

    [Fact]
    public async Task AcceptInvitationAsync_Should_Return_Fail_When_Membership_Already_Exists()
    {
        var invitation = BuildPendingInvitation();
        var user = new ApplicationUser
        {
            Id = 10,
            PublicId = Guid.NewGuid(),
            Email = invitation.Email,
            UserName = invitation.Email
        };

        _invitationRepoMock
            .Setup(x => x.GetByPublicIdAsync(invitation.PublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        _userRepoMock
            .Setup(x => x.GetUserByEmailAsync(invitation.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _membershipRepoMock
            .Setup(x => x.GetMembershipAsync(invitation.TenantId, user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TenantMembership
            {
                TenantId = invitation.TenantId,
                UserId = user.Id,
                IsActive = true
            });

        var result = await _sut.AcceptInvitationAsync(
            invitation.PublicId.ToString(),
            invitation.Version.ToString(),
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("User already belongs to this tenant.");
    }

    [Fact]
    public async Task AcceptInvitationAsync_Should_Create_Membership_And_Update_Invitation_When_Request_Is_Valid()
    {
        var invitation = BuildPendingInvitation();
        var user = new ApplicationUser
        {
            Id = 10,
            PublicId = Guid.NewGuid(),
            Email = invitation.Email,
            UserName = invitation.Email
        };

        TenantMembership? savedMembership = null;

        _invitationRepoMock
            .Setup(x => x.GetByPublicIdAsync(invitation.PublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        _userRepoMock
            .Setup(x => x.GetUserByEmailAsync(invitation.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _membershipRepoMock
            .Setup(x => x.GetMembershipAsync(invitation.TenantId, user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantMembership?)null);

        _membershipRepoMock
            .Setup(x => x.AddMembershipAsync(It.IsAny<TenantMembership>(), It.IsAny<CancellationToken>()))
            .Callback<TenantMembership, CancellationToken>((m, _) => savedMembership = m)
            .Returns(Task.CompletedTask);

        var originalVersion = invitation.Version;

        var result = await _sut.AcceptInvitationAsync(
            invitation.PublicId.ToString(),
            invitation.Version.ToString(),
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();
        result.Message.Should().Be("Invitation accepted successfully.");

        savedMembership.Should().NotBeNull();
        savedMembership!.TenantId.Should().Be(invitation.TenantId);
        savedMembership.UserId.Should().Be(user.Id);
        savedMembership.Role.Should().Be(invitation.Role);
        savedMembership.IsActive.Should().BeTrue();

        invitation.Status.Should().Be(InvitationStatus.Accepted);
        invitation.AcceptedAt.Should().NotBeNull();
        invitation.Version.Should().Be(originalVersion + 1);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static TenantInvitation BuildPendingInvitation(int version = 1)
    {
        return new TenantInvitation
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            Email = "user@test.com",
            TenantId = 100,
            Tenant = new Tenant
            {
                Id = 100,
                PublicId = Guid.NewGuid(),
                Name = "FinTrack"
            },
            Role = TenantRole.Member,
            Status = InvitationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ExpiredAt = DateTime.UtcNow.AddDays(7),
            Version = version,
            CreatedByUserId = 999,
            CreatedByUser = new ApplicationUser
            {
                Id = 999,
                PublicId = Guid.NewGuid(),
                Email = "admin@test.com",
                UserName = "admin@test.com"
            }
        };
    }
}