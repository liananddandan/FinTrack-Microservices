using AutoFixture.Xunit2;
using FluentAssertions;
using IdentityService.Common.Results;
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
                    It.IsAny<string>(), It.IsAny<long>(), 
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
        
        var roleName = $"Admin_{tenantName}";
        userDomainServiceMock.Setup(u
                => u.CreateRoleInnerAsync(roleName, It.IsAny<CancellationToken>()))
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
            TenantId = 2002
        };
        userDomainServiceMock.Setup(u
                => u.CreateUserOrThrowInnerAsync(It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<long>(), 
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
            TenantId = 2002
        };
        userDomainServiceMock.Setup(u
                => u.CreateUserOrThrowInnerAsync(It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<long>(), 
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
            m.Publish(It.IsAny<TenantRegisteredEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task InviteUserForTenantAsync_ShouldReturnFail_WhenUserNotExist(
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        string adminPublicId, string adminJwtVersion, string tenantPublicId,
        string adminRoleInTenant, List<string> emails, 
        TenantService sut)
    {
        // Arrange
        userDomainServiceMock
            .Setup(uds => uds.GetUserByPublicIdIncludeTenantAsync(adminPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);
        
        // Act
        var result = await sut.InviteUserForTenantAsync(adminPublicId, adminJwtVersion, tenantPublicId, adminRoleInTenant, emails);
        
        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserNotFound);
    }
    
    [Theory, AutoMoqData]
    public async Task InviteUserForTenantAsync_ShouldReturnFail_WhenUserTenantNotExist(
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        string adminPublicId, string adminJwtVersion, string tenantPublicId,
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
        var result = await sut.InviteUserForTenantAsync(adminPublicId, adminJwtVersion, tenantPublicId, adminRoleInTenant, emails);
        
        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserTenantInfoMissed);
    }
    
    [Theory, AutoMoqData]
    public async Task InviteUserForTenantAsync_ShouldReturnFail_WhenUserTenantNotMatch(
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        string adminPublicId, string adminJwtVersion, string tenantPublicId,
        string adminRoleInTenant, List<string> emails, 
        ApplicationUser user,
        TenantService sut)
    {
        // Arrange
        userDomainServiceMock
            .Setup(uds => uds.GetUserByPublicIdIncludeTenantAsync(adminPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        
        // Act
        var result = await sut.InviteUserForTenantAsync(adminPublicId, adminJwtVersion, tenantPublicId, adminRoleInTenant, emails);
        
        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserTenantInfoMissed);
    }
    
    [Theory, AutoMoqData]
    public async Task InviteUserForTenantAsync_ShouldReturnFail_WhenJwtVersionNotMatch(
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        string adminJwtVersion,
        string adminRoleInTenant, List<string> emails, 
        ApplicationUser user,
        TenantService sut)
    {
        // Arrange
        userDomainServiceMock
            .Setup(uds => uds.GetUserByPublicIdIncludeTenantAsync(user.PublicId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        
        // Act
        var result = await sut.InviteUserForTenantAsync(user.PublicId.ToString(), adminJwtVersion, 
            user.Tenant!.PublicId.ToString(), adminRoleInTenant, emails);
        
        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.Token.JwtTokenVersionInvalid);
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
            .Setup(uds => uds.GetUserByPublicIdIncludeTenantAsync(user.PublicId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        userDomainServiceMock
            .Setup(uds => uds.GetRoleInnerAsync(user, CancellationToken.None))
            .ReturnsAsync((string?)null);
        
        // Act
        var result = await sut.InviteUserForTenantAsync(user.PublicId.ToString(), user.JwtVersion.ToString(), 
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
            .Setup(uds => uds.GetUserByPublicIdIncludeTenantAsync(user.PublicId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        userDomainServiceMock
            .Setup(uds => uds.GetRoleInnerAsync(user, CancellationToken.None))
            .ReturnsAsync(role);
        
        // Act
        var result = await sut.InviteUserForTenantAsync(user.PublicId.ToString(), user.JwtVersion.ToString(), 
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
            .Setup(uds => uds.GetUserByPublicIdIncludeTenantAsync(user.PublicId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        userDomainServiceMock
            .Setup(uds => uds.GetRoleInnerAsync(user, CancellationToken.None))
            .ReturnsAsync(role);
        
        // Act
        var result = await sut.InviteUserForTenantAsync(user.PublicId.ToString(), user.JwtVersion.ToString(), 
            user.Tenant!.PublicId.ToString(), role, emails);
        
        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserWithoutAdminRolePermission);
    }
    
    [Theory, AutoMoqData]
    public async Task InviteUserForTenantAsync_ShouldReturnFail_WhenEverythingMatch(
        [Frozen] Mock<IUserDomainService> userDomainServiceMock, List<string> emails, 
        ApplicationUser user,
        TenantService sut)
    {
        // Arrange
        var role = $"Admin_{user.Tenant!.Name}";
        userDomainServiceMock
            .Setup(uds => uds.GetUserByPublicIdIncludeTenantAsync(user.PublicId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        userDomainServiceMock
            .Setup(uds => uds.GetRoleInnerAsync(user, CancellationToken.None))
            .ReturnsAsync(role);
        
        // Act
        var result = await sut.InviteUserForTenantAsync(user.PublicId.ToString(), user.JwtVersion.ToString(), 
            user.Tenant!.PublicId.ToString(), role, emails);
        
        // Assert
        result.Success.Should().BeTrue();
        result.Code.Should().BeEquivalentTo(ResultCodes.Tenant.InvitationUsersStartSuccess);
    }
}