using AutoFixture.Xunit2;
using FluentAssertions;
using IdentityService.Common.Status;
using IdentityService.Domain.Entities;
using IdentityService.Common.DTOs;
using IdentityService.Events;
using IdentityService.Repositories.Interfaces;
using IdentityService.Services;
using IdentityService.Services.Interfaces;
using IdentityService.Tests.Attributes;
using IdentityService.Tests.Extensions;
using MediatR;
using Moq;
using SharedKernel.Common.Exceptions;
using SharedKernel.Common.Results;

namespace IdentityService.Tests.Services;

public class TenantServiceTests
{
    [Theory, AutoMoqData]
    public async Task RegisterTenantAsync_ShouldThrowException_WhenCreateUserFailed(
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        TenantService sut,
        string tenantName,
        string adminName,
        string adminEmail)
    {
        // Arrange
        unitOfWorkMock.SetupExecuteWithTransaction<ServiceResult<RegisterTenantResult>>();

        userDomainServiceMock.Setup(u
                => u.CreateUserOrThrowInnerAsync(It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>(),
                    It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UserCreateException("UserCreateException"));

        // Act
        var result = await sut.RegisterTenantAsync(tenantName, adminName, adminEmail);

        // Assert
        result.Data.Should().BeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.Tenant.RegisterTenantException);
    }

    [Theory, AutoMoqData]
    public async Task RegisterTenantAsync_ShouldReturnFail_WhenCreateRoleFailed(
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        TenantService sut,
        string tenantName,
        string adminName,
        string adminEmail)
    {
        // Arrange
        unitOfWorkMock.SetupExecuteWithTransaction<ServiceResult<RegisterTenantResult>>();

        userDomainServiceMock.Setup(u
                => u.CreateRoleInnerAsync(It.IsAny<ApplicationRole>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RoleStatus.CreateFailed);

        // Act
        var result = await sut.RegisterTenantAsync(tenantName, adminName, adminEmail);

        // Assert
        result.Data.Should().BeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.Tenant.RegisterTenantRoleCreateFailed);
        result.Message.Should().BeEquivalentTo("Role creation failed");
    }

    [Theory, AutoMoqData]
    public async Task RegisterTenantAsync_ShouldReturnFail_WhenAddUserToRoleFailed(
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        TenantService sut,
        string tenantName,
        string adminName,
        string adminEmail)
    {
        // Arrange
        unitOfWorkMock.SetupExecuteWithTransaction<ServiceResult<RegisterTenantResult>>();

        var randomPassword = "password";
        var user = new ApplicationUser
        {
            Id = 3003,
            Email = adminEmail,
            UserName = adminName,
            TenantId = 2002,
            RoleId = 1
        };
        userDomainServiceMock.Setup(u
                => u.CreateUserOrThrowInnerAsync(It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<long>(),
                    It.IsAny<long>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync((user, randomPassword));
        var roleName = $"Admin_{tenantName}";
        userDomainServiceMock.Setup(u
                => u.CreateRoleInnerAsync(roleName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RoleStatus.CreateSuccess);
        userDomainServiceMock.Setup(u =>
                u.AddUserToRoleInnerAsync(user, roleName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RoleStatus.AddRoleToUserFailed);

        // Act
        var result = await sut.RegisterTenantAsync(tenantName, adminName, adminEmail);

        // Assert
        result.Data.Should().BeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.Tenant.RegisterTenantRoleGrantFailed);
        result.Message.Should().BeEquivalentTo("Role grant failed");
    }

    [Theory, AutoMoqData]
    public async Task RegisterTenantAsync_ShouldReturnTrue_WhenRegisterSuccess(
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        [Frozen] Mock<IMediator> mediatorMock,
        TenantService sut,
        string tenantName,
        string adminName,
        string adminEmail)
    {
        // Arrange
        unitOfWorkMock.SetupExecuteWithTransaction<ServiceResult<RegisterTenantResult>>();

        var randomPassword = "password";
        var user = new ApplicationUser
        {
            Id = 3003,
            Email = adminEmail,
            UserName = adminName,
            TenantId = 2002,
            RoleId = 1
        };
        userDomainServiceMock.Setup(u
                => u.CreateUserOrThrowInnerAsync(It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<long>(),
                    It.IsAny<long>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync((user, randomPassword));
        var roleName = "Admin";
        userDomainServiceMock.Setup(u
                => u.CreateRoleInnerAsync(roleName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RoleStatus.CreateSuccess);
        userDomainServiceMock.Setup(u =>
                u.AddUserToRoleInnerAsync(user, roleName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RoleStatus.AddRoleToUserSuccess);

        // Act
        var result = await sut.RegisterTenantAsync(tenantName, adminName, adminEmail);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.AdminEmail.Should().Be(adminEmail);
        result.Data.TemporaryPassword.Should().Be(randomPassword);
        result.Code.Should().BeEquivalentTo(ResultCodes.Tenant.RegisterTenantSuccess);
        result.Message.Should().BeEquivalentTo("Registered tenant successfully");
        mediatorMock.Verify(m =>
                m.Publish(It.IsAny<UserRegisteredEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task InviteUserForTenantAsync_ShouldReturnFail_WhenUserNotExist(
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        string adminPublicId, string tenantPublicId,
        string adminRoleInTenant, List<string> emails,
        TenantService sut)
    {
        // Arrange
        userDomainServiceMock
            .Setup(uds => uds.GetUserByPublicIdIncludeTenantAsync(adminPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await sut.InviteUserForTenantAsync(adminPublicId, tenantPublicId, adminRoleInTenant, emails);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserNotFound);
    }

    [Theory, AutoMoqData]
    public async Task InviteUserForTenantAsync_ShouldReturnFail_WhenUserTenantNotExist(
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        string adminPublicId, string tenantPublicId,
        string adminRoleInTenant, List<string> emails,
        ApplicationUser user,
        TenantService sut)
    {
        // Arrange
        user.Tenant = null;
        userDomainServiceMock
            .Setup(uds => uds.GetUserByPublicIdIncludeTenantAsync(adminPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await sut.InviteUserForTenantAsync(adminPublicId, tenantPublicId,
            adminRoleInTenant, emails);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserTenantInfoMissed);
    }

    [Theory, AutoMoqData]
    public async Task InviteUserForTenantAsync_ShouldReturnFail_WhenUserTenantNotMatch(
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        string adminPublicId, string tenantPublicId,
        string adminRoleInTenant, List<string> emails,
        ApplicationUser user,
        TenantService sut)
    {
        // Arrange
        userDomainServiceMock
            .Setup(uds => uds.GetUserByPublicIdIncludeTenantAsync(adminPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await sut.InviteUserForTenantAsync(adminPublicId, tenantPublicId,
            adminRoleInTenant, emails);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserTenantInfoMissed);
    }
    
    [Theory, AutoMoqData]
    public async Task InviteUserForTenantAsync_ShouldReturnFail_WhenRoleNotExist(
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        string adminRoleInTenant, List<string> emails,
        ApplicationUser user,
        TenantService sut)
    {
        // Arrange
        userDomainServiceMock
            .Setup(uds =>
                uds.GetUserByPublicIdIncludeTenantAsync(user.PublicId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        userDomainServiceMock
            .Setup(uds => uds.GetUserRoleInnerAsync(user, CancellationToken.None))
            .ReturnsAsync((string?)null);

        // Act
        var result = await sut.InviteUserForTenantAsync(user.PublicId.ToString(),
            user.Tenant!.PublicId.ToString(), adminRoleInTenant, emails);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserCouldNotFindRole);
    }

    [Theory, AutoMoqData]
    public async Task InviteUserForTenantAsync_ShouldReturnFail_WhenRoleNotMatch(
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        string adminRoleInTenant, List<string> emails,
        ApplicationUser user,
        TenantService sut)
    {
        // Arrange
        var role = "test_role";
        userDomainServiceMock
            .Setup(uds =>
                uds.GetUserByPublicIdIncludeTenantAsync(user.PublicId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        userDomainServiceMock
            .Setup(uds => uds.GetUserRoleInnerAsync(user, CancellationToken.None))
            .ReturnsAsync(role);

        // Act
        var result = await sut.InviteUserForTenantAsync(user.PublicId.ToString(), 
            user.Tenant!.PublicId.ToString(), adminRoleInTenant, emails);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserWithoutAdminRolePermission);
    }

    [Theory, AutoMoqData]
    public async Task InviteUserForTenantAsync_ShouldReturnFail_WhenRoleNotMatchTenant(
        [Frozen] Mock<IUserDomainService> userDomainServiceMock, List<string> emails,
        ApplicationUser user,
        TenantService sut)
    {
        // Arrange
        var role = "test_role";
        userDomainServiceMock
            .Setup(uds =>
                uds.GetUserByPublicIdIncludeTenantAsync(user.PublicId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        userDomainServiceMock
            .Setup(uds => uds.GetUserRoleInnerAsync(user, CancellationToken.None))
            .ReturnsAsync(role);

        // Act
        var result = await sut.InviteUserForTenantAsync(user.PublicId.ToString(),
            user.Tenant!.PublicId.ToString(), role, emails);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserWithoutAdminRolePermission);
    }

    [Theory, AutoMoqData]
    public async Task InviteUserForTenantAsync_ShouldReturnSuccess_WhenEverythingMatch(
        [Frozen] Mock<IUserDomainService> userDomainServiceMock, List<string> emails,
        ApplicationUser user,
        TenantService sut)
    {
        // Arrange
        var role = $"Admin_{user.Tenant!.Name}";
        userDomainServiceMock
            .Setup(uds =>
                uds.GetUserByPublicIdIncludeTenantAsync(user.PublicId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        userDomainServiceMock
            .Setup(uds => uds.GetUserRoleInnerAsync(user, CancellationToken.None))
            .ReturnsAsync(role);

        // Act
        var result = await sut.InviteUserForTenantAsync(user.PublicId.ToString(),
            user.Tenant!.PublicId.ToString(), role, emails);

        // Assert
        result.Success.Should().BeTrue();
        result.Code.Should().BeEquivalentTo(ResultCodes.Tenant.InvitationUsersStartSuccess);
    }

    [Theory, AutoMoqData]
    public async Task ReceiveInviteForTenantAsync_ShouldReturnFalse_WhenInvitationNotExist(
        [Frozen] Mock<ITenantInvitationService> tenantInvitationServiceMock,
        string invitationPublicId,
        TenantService sut
    )
    {
        // Arrange
        tenantInvitationServiceMock
            .Setup(tis => tis.GetTenantInvitationByPublicIdAsync(invitationPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TenantInvitation>.Fail("not exist", "not exist"));

        // Act
        var result =
            await sut.ReceiveInviteForTenantAsync(invitationPublicId, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
    }


    [Theory, AutoMoqData]
    public async Task ReceiveInviteForTenantAsync_ShouldReturnFalse_WhenInvitationExpired(
        [Frozen] Mock<ITenantInvitationService> tenantInvitationServiceMock,
        string invitationPublicId,
        TenantInvitation invitation,
        TenantService sut
    )
    {
        // Arrange
        invitation.ExpiredAt = DateTime.UtcNow.AddDays(-1);
        tenantInvitationServiceMock
            .Setup(tis => tis.GetTenantInvitationByPublicIdAsync(invitationPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TenantInvitation>.Ok(invitation, "exist", "exist"));

        // Act
        var result =
            await sut.ReceiveInviteForTenantAsync(invitationPublicId, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.Tenant.InvitationExpired);
    }

    [Theory, AutoMoqData]
    public async Task ReceiveInviteForTenantAsync_ShouldReturnFalse_WhenStatusRevoked(
        [Frozen] Mock<ITenantInvitationService> tenantInvitationServiceMock,
        string invitationPublicId,
        TenantInvitation invitation,
        TenantService sut
    )
    {
        // Arrange
        invitation.Version = 4;
        invitation.ExpiredAt = DateTime.UtcNow.AddDays(7);
        invitation.Status = InvitationStatus.Revoked;
        tenantInvitationServiceMock
            .Setup(tis => tis.GetTenantInvitationByPublicIdAsync(invitationPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TenantInvitation>.Ok(invitation, "exist", "exist"));

        // Act
        var result =
            await sut.ReceiveInviteForTenantAsync(invitationPublicId, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.Tenant.InvitationRevoked);
    }

    [Theory, AutoMoqData]
    public async Task ReceiveInviteForTenantAsync_ShouldReturnFalse_WhenStatusAccepted(
        [Frozen] Mock<ITenantInvitationService> tenantInvitationServiceMock,
        string invitationPublicId,
        TenantInvitation invitation,
        TenantService sut
    )
    {
        // Arrange
        invitation.Version = 4;
        invitation.ExpiredAt = DateTime.UtcNow.AddDays(7);
        invitation.Status = InvitationStatus.Accepted;
        tenantInvitationServiceMock
            .Setup(tis => tis.GetTenantInvitationByPublicIdAsync(invitationPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TenantInvitation>.Ok(invitation, "exist", "exist"));

        // Act
        var result =
            await sut.ReceiveInviteForTenantAsync(invitationPublicId, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.Tenant.InvitationHasAccepted);
    }

    [Theory, AutoMoqData]
    public async Task ReceiveInviteForTenantAsync_ShouldReturnFalse_WhenTenantNotExist(
        [Frozen] Mock<ITenantInvitationService> tenantInvitationServiceMock,
        [Frozen] Mock<ITenantRepo> tenantRepoMock,
        string invitationPublicId,
        TenantInvitation invitation,
        TenantService sut
    )
    {
        // Arrange
        invitation.Version = 4;
        invitation.ExpiredAt = DateTime.UtcNow.AddDays(7);
        invitation.Status = InvitationStatus.Pending;
        tenantInvitationServiceMock
            .Setup(tis => tis.GetTenantInvitationByPublicIdAsync(invitationPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TenantInvitation>.Ok(invitation, "exist", "exist"));
        tenantRepoMock
            .Setup(tr => tr.GetTenantByPublicIdAsync(invitation.TenantPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as Tenant);

        // Act
        var result =
            await sut.ReceiveInviteForTenantAsync(invitationPublicId, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.Tenant.InvitationWithAInvalidTenant);
    }

    [Theory, AutoMoqData]
    public async Task ReceiveInviteForTenantAsync_ShouldReturnFalse_WhenThrowException(
        [Frozen] Mock<ITenantInvitationService> tenantInvitationServiceMock,
        [Frozen] Mock<ITenantRepo> tenantRepoMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        string invitationPublicId,
        TenantInvitation invitation,
        Tenant tenant,
        TenantService sut
    )
    {
        // Arrange
        unitOfWorkMock.SetupExecuteWithTransaction<ServiceResult<bool>>();
        invitation.Version = 4;
        invitation.ExpiredAt = DateTime.UtcNow.AddDays(7);
        invitation.Status = InvitationStatus.Pending;
        tenantInvitationServiceMock
            .Setup(tis => tis.GetTenantInvitationByPublicIdAsync(invitationPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TenantInvitation>.Ok(invitation, "exist", "exist"));
        tenantRepoMock
            .Setup(tr => tr.GetTenantByPublicIdAsync(invitation.TenantPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);
        userDomainServiceMock.Setup(uds => uds.CreateUserOrThrowInnerAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>(),It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UserCreateException("user creation failed"));

        // Act
        var result =
            await sut.ReceiveInviteForTenantAsync(invitationPublicId, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.Tenant.InvitationCreateFailed);
    }

    [Theory, AutoMoqData]
    public async Task ReceiveInviteForTenantAsync_ShouldReturnFalse_WhenRoleNotExistAndCreateFailed(
        [Frozen] Mock<ITenantInvitationService> tenantInvitationServiceMock,
        [Frozen] Mock<ITenantRepo> tenantRepoMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        ApplicationUser user,
        string password,
        string invitationPublicId,
        TenantInvitation invitation,
        Tenant tenant,
        TenantService sut
    )
    {
        // Arrange
        unitOfWorkMock.SetupExecuteWithTransaction<ServiceResult<bool>>();
        invitation.Version = 4;
        invitation.ExpiredAt = DateTime.UtcNow.AddDays(7);
        invitation.Status = InvitationStatus.Pending;
        tenantInvitationServiceMock
            .Setup(tis => tis.GetTenantInvitationByPublicIdAsync(invitationPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TenantInvitation>.Ok(invitation, "exist", "exist"));
        tenantRepoMock
            .Setup(tr => tr.GetTenantByPublicIdAsync(invitation.TenantPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);
        userDomainServiceMock.Setup(uds => uds.CreateUserOrThrowInnerAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(),It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((user, password));
        userDomainServiceMock.Setup(uds => uds.IsRoleExistAsync(invitation.Role, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RoleStatus.RoleNotExist);
        userDomainServiceMock.Setup(uds => uds.CreateRoleInnerAsync(invitation.Role, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RoleStatus.CreateFailed);

        // Act
        var result =
            await sut.ReceiveInviteForTenantAsync(invitationPublicId, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.RoleCreatedFailed);
    }

    [Theory, AutoMoqData]
    public async Task ReceiveInviteForTenantAsync_ShouldReturnFalse_WhenAddUserRoleFailed(
        [Frozen] Mock<ITenantInvitationService> tenantInvitationServiceMock,
        [Frozen] Mock<ITenantRepo> tenantRepoMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        ApplicationUser user,
        string password,
        string invitationPublicId,
        TenantInvitation invitation,
        Tenant tenant,
        TenantService sut
    )
    {
        // Arrange
        unitOfWorkMock.SetupExecuteWithTransaction<ServiceResult<bool>>();
        invitation.Version = 4;
        invitation.ExpiredAt = DateTime.UtcNow.AddDays(7);
        invitation.Status = InvitationStatus.Pending;
        tenantInvitationServiceMock
            .Setup(tis => tis.GetTenantInvitationByPublicIdAsync(invitationPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TenantInvitation>.Ok(invitation, "exist", "exist"));
        tenantRepoMock
            .Setup(tr => tr.GetTenantByPublicIdAsync(invitation.TenantPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);
        userDomainServiceMock.Setup(uds => uds.CreateUserOrThrowInnerAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>(),It.IsAny<CancellationToken>()))
            .ReturnsAsync((user, password));
        userDomainServiceMock.Setup(uds => uds.IsRoleExistAsync(invitation.Role, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RoleStatus.RoleNotExist);
        userDomainServiceMock.Setup(uds => uds.CreateRoleInnerAsync(invitation.Role, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RoleStatus.CreateSuccess);
        userDomainServiceMock.Setup(uds =>
                uds.AddUserToRoleInnerAsync(user, invitation.Role, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RoleStatus.AddRoleToUserFailed);

        // Act
        var result =
            await sut.ReceiveInviteForTenantAsync(invitationPublicId, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.RoleGrantUserFailed);
    }

    [Theory, AutoMoqData]
    public async Task ReceiveInviteForTenantAsync_ShouldReturnTrue_WhenSendInvitationSuccess(
        [Frozen] Mock<ITenantInvitationService> tenantInvitationServiceMock,
        [Frozen] Mock<ITenantRepo> tenantRepoMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        ApplicationUser user,
        string password,
        string invitationPublicId,
        TenantInvitation invitation,
        Tenant tenant,
        TenantService sut
    )
    {
        // Arrange
        unitOfWorkMock.SetupExecuteWithTransaction<ServiceResult<bool>>();
        invitation.Version = 4;
        invitation.ExpiredAt = DateTime.UtcNow.AddDays(7);
        invitation.Status = InvitationStatus.Pending;
        tenantInvitationServiceMock
            .Setup(tis => tis.GetTenantInvitationByPublicIdAsync(invitationPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TenantInvitation>.Ok(invitation, "exist", "exist"));
        tenantRepoMock
            .Setup(tr => tr.GetTenantByPublicIdAsync(invitation.TenantPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);
        userDomainServiceMock.Setup(uds => uds.CreateUserOrThrowInnerAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>(),It.IsAny<CancellationToken>()))
            .ReturnsAsync((user, password));
        userDomainServiceMock.Setup(uds => uds.IsRoleExistAsync(invitation.Role, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RoleStatus.RoleNotExist);
        userDomainServiceMock.Setup(uds => uds.CreateRoleInnerAsync(invitation.Role, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RoleStatus.CreateSuccess);
        userDomainServiceMock.Setup(uds =>
                uds.AddUserToRoleInnerAsync(user, invitation.Role, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RoleStatus.AddRoleToUserSuccess);
        tenantInvitationServiceMock.Setup(tis => tis.UpdateTenantInvitationAsync(invitation, CancellationToken.None))
            .ReturnsAsync(ServiceResult<bool>.Ok(true, "update", "update"));

        // Act
        var result =
            await sut.ReceiveInviteForTenantAsync(invitationPublicId, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Code.Should().BeEquivalentTo(ResultCodes.Tenant.InvitationReceiveSuccess);
    }

    [Theory, AutoMoqData]
    public async Task GetUsersForTenantAsync_ShouldReturnFail_WhenAdminNotFound(
        [Frozen]Mock<IUserDomainService> userDomainServiceMock,
        string adminPublicId, 
        string tenantPublicId,
        string adminRoleInTenant,
        TenantService sut)
    {
        // Arrange
        userDomainServiceMock
            .Setup(uds => uds.GetUserByPublicIdIncludeTenantAndRoleAsync(adminPublicId, CancellationToken.None))
            .ReturnsAsync((ApplicationUser?)null);
        
        // Act
        var result = await sut.GetUsersForTenantAsync(adminPublicId, tenantPublicId, adminRoleInTenant, CancellationToken.None);
        
        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserNotFound);
    }
    
    [Theory, AutoMoqData]
    public async Task GetUsersForTenantAsync_ShouldReturnFail_WhenRoleIncorecct(
        [Frozen]Mock<IUserDomainService> userDomainServiceMock,
        string adminPublicId, 
        string tenantPublicId,
        string adminRoleInTenant,
        ApplicationUser user,
        TenantService sut)
    {
        // Arrange
        userDomainServiceMock
            .Setup(uds => uds.GetUserByPublicIdIncludeTenantAndRoleAsync(adminPublicId, CancellationToken.None))
            .ReturnsAsync(user);
        
        // Act
        var result = await sut.GetUsersForTenantAsync(adminPublicId, tenantPublicId, adminRoleInTenant, CancellationToken.None);
        
        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.Tenant.UserDoNotHavePermissionToQueryTenant);
    }
    
    [Theory, AutoMoqData]
    public async Task GetUsersForTenantAsync_ShouldReturnFail_WhenRoleIncorrect(
        [Frozen]Mock<IUserDomainService> userDomainServiceMock,
        string adminPublicId, 
        string tenantPublicId,
        string adminRoleInTenant,
        ApplicationUser user,
        TenantService sut)
    {
        // Arrange
        user.Role.Name = "Admin_Test";
        userDomainServiceMock
            .Setup(uds => uds.GetUserByPublicIdIncludeTenantAndRoleAsync(adminPublicId, CancellationToken.None))
            .ReturnsAsync(user);
        
        // Act
        var result = await sut.GetUsersForTenantAsync(adminPublicId, tenantPublicId, adminRoleInTenant, CancellationToken.None);
        
        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.Tenant.UserDoNotHavePermissionToQueryTenant);
    }
    
    [Theory, AutoMoqData]
    public async Task GetUsersForTenantAsync_ShouldReturnFail_WhenPermissonNotBelongToTenantIncorrect(
        [Frozen]Mock<IUserDomainService> userDomainServiceMock,
        string adminPublicId, 
        string tenantPublicId,
        string adminRoleInTenant,
        ApplicationUser user,
        TenantService sut)
    {
        // Arrange
        user.Tenant.Name = "Test_Role";
        user.Role.Name = "Admin_Test";
        userDomainServiceMock
            .Setup(uds => uds.GetUserByPublicIdIncludeTenantAndRoleAsync(adminPublicId, CancellationToken.None))
            .ReturnsAsync(user);
        
        // Act
        var result = await sut.GetUsersForTenantAsync(adminPublicId, tenantPublicId, adminRoleInTenant, CancellationToken.None);
        
        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.Tenant.UserDoNotHavePermissionToQueryTenant);
    }
    
    [Theory, AutoMoqData]
    public async Task GetUsersForTenantAsync_ShouldReturnFail_WhenTenantNotMatchJwtToken(
        [Frozen]Mock<IUserDomainService> userDomainServiceMock,
        string adminPublicId, 
        string tenantPublicId,
        ApplicationUser user,
        TenantService sut)
    {
        // Arrange
        user.Tenant.Name = "Test_Role";
        user.Role.Name = "Admin_Test_Role";
        var adminRoleInTenant = "Admin_Test_Role";
        userDomainServiceMock
            .Setup(uds => uds.GetUserByPublicIdIncludeTenantAndRoleAsync(adminPublicId, CancellationToken.None))
            .ReturnsAsync(user);
        
        // Act
        var result = await sut.GetUsersForTenantAsync(adminPublicId, tenantPublicId, adminRoleInTenant, CancellationToken.None);
        
        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.Tenant.UserDoNotBelongToTenant);
    }
    
    [Theory, AutoMoqData]
    public async Task GetUsersForTenantAsync_ShouldReturnSuccess_WhenEveryThingMatch(
        [Frozen]Mock<IUserDomainService> userDomainServiceMock,
        string adminPublicId, 
        ApplicationUser user,
        List<ApplicationUser> users,
        TenantService sut)
    {
        // Arrange
        user.Tenant.Name = "Test_Role";
        user.Role.Name = "Admin_Test_Role";
        user.TenantId = 1;
        var tenantPublicId = user.Tenant.PublicId.ToString();
        var adminRoleInTenant = "Admin_Test_Role";
        userDomainServiceMock
            .Setup(uds => uds.GetUserByPublicIdIncludeTenantAndRoleAsync(adminPublicId, CancellationToken.None))
            .ReturnsAsync(user);
        userDomainServiceMock
            .Setup(uds => uds.GetAllUsersInTenantIncludeRoleAsync(user.TenantId, CancellationToken.None))
            .ReturnsAsync(users);
        
        // Act
        var result = await sut.GetUsersForTenantAsync(adminPublicId, tenantPublicId, adminRoleInTenant, CancellationToken.None);
        
        // Assert
        result.Success.Should().BeTrue();
        result.Code.Should().BeEquivalentTo(ResultCodes.Tenant.GetAllUserInTenantSuccess);
        result.Data.Should().NotBeNull();
        result.Data.Count().Should().Be(users.Count);
    }
}