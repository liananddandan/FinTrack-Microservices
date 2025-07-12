using AutoFixture.Xunit2;
using FluentAssertions;
using IdentityService.Common.Status;
using IdentityService.Domain.Entities;
using IdentityService.Services;
using IdentityService.Tests.Attributes;
using Microsoft.AspNetCore.Identity;
using MockQueryable;
using Moq;
using SharedKernel.Common.Exceptions;
using SharedKernel.Common.Results;

namespace IdentityService.Tests.Services;

public class UserDomainServiceTests
{
    [Theory, AutoMoqData]
    public async Task CreateUserInnerAsync_ShouldReturnUser_WhenCreateSuccess(
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        UserDomainService sut,
        string userName,
        string userEmail,
        long tenantId)
    {
        // Arrange
        long expectedUserId = 1001;
        userManagerMock.Setup(usm =>
                usm.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .Callback<ApplicationUser, string>((user, _) =>
            {
                user.Id = expectedUserId;
            })
            .ReturnsAsync(IdentityResult.Success);
        
        // Act
        var (userResult, pwdResult) = await sut.CreateUserOrThrowInnerAsync(userName, userEmail, tenantId);
        
        // Assert
        userResult.Should().NotBeNull();
        userResult.Id.Should().Be(expectedUserId);
        userResult.Email.Should().Be(userEmail);
        userResult.UserName.Should().Be(userName);
        userResult.TenantId.Should().Be(tenantId);
        userResult.PublicId.Should().NotBeEmpty();
        pwdResult.Should().NotBeNull();
    }

    [Theory, AutoMoqData]
    public async Task CreateUserInnerAsync_ShouldThrowException_WhenCreateFail(
        [Frozen]Mock<UserManager<ApplicationUser>> userManagerMock,
        UserDomainService sut,
        string userName,
        string userEmail,
        long tenantId,
        IdentityError[] expectedErrors)
    {
        // Arrange
        userManagerMock.Setup(usm =>
                usm.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(expectedErrors));
        
        // Act
        var ex = await Assert.ThrowsAsync<UserCreateException>(() =>
            sut.CreateUserOrThrowInnerAsync(userName, userEmail, tenantId));
        
        // Assert
        ex.Should().NotBeNull();
        ex.Message.Should().Contain("CreateUserAsync failed");
        ex.Code.Should().BeEquivalentTo("UserCreateException");
        ex.StatusCode.Should().Be(500);
    }

    [Theory, AutoMoqData]
    public async Task CreateRoleInnerAsync_ShouldReturnAlreadyExist_WhenRoleExisted(
        [Frozen] Mock<RoleManager<ApplicationRole>> roleManagerMock,
        UserDomainService sut,
        string roleName)
    {
        // Arrange
        roleManagerMock.Setup(roleManager => roleManager.RoleExistsAsync(roleName.ToUpperInvariant()))
            .ReturnsAsync(true);
        
        // Act
        var result = await sut.CreateRoleInnerAsync(roleName);
        
        // Assert
        result.Should().Be(RoleStatus.RoleAlreadyExist);
    }

    [Theory, AutoMoqData]
    public async Task CreateRoleInnterAsync_ShouldReturnSuccess_WhenRoleNotExistAndCreateSuccess(
        [Frozen] Mock<RoleManager<ApplicationRole>> roleManagerMock,
        UserDomainService sut,
        string roleName)
    {
        // Arrange
        roleManagerMock.Setup(roleManager => roleManager.RoleExistsAsync(roleName)).ReturnsAsync(false);
        roleManagerMock.Setup(rolemanager => rolemanager.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Success);
        
        // Act
        var result = await sut.CreateRoleInnerAsync(roleName);
        
        // Assert
        result.Should().Be(RoleStatus.CreateSuccess);
    }
    
    [Theory, AutoMoqData]
    public async Task CreateRoleInnterAsync_ShouldReturnFail_WhenRoleNotExistAndCreateFailed(
        [Frozen] Mock<RoleManager<ApplicationRole>> roleManagerMock,
        UserDomainService sut,
        string roleName,
        IdentityError[] expectedErrors)
    {
        // Arrange
        roleManagerMock.Setup(roleManager => roleManager.RoleExistsAsync(roleName)).ReturnsAsync(false);
        roleManagerMock.Setup(rolemanager => rolemanager.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Failed(expectedErrors));
        
        // Act
        var result = await sut.CreateRoleInnerAsync(roleName);
        
        // Assert
        result.Should().Be(RoleStatus.CreateFailed);
    }
    
    [Theory, AutoMoqData]
    public async Task AddUserToRoleInnerAsync_ShouldReturnRoleNotExist_WhenRoleNotExisted(
        [Frozen] Mock<RoleManager<ApplicationRole>> roleManagerMock,
        UserDomainService sut,
        string roleName,
        ApplicationUser user)
    {
        // Arrange
        roleManagerMock.Setup(roleManager => roleManager.RoleExistsAsync(roleName))
            .ReturnsAsync(false);
        
        // Act
        var result = await sut.AddUserToRoleInnerAsync(user, roleName);
        
        // Assert
        result.Should().Be(RoleStatus.RoleNotExist);
    }
    
    [Theory, AutoMoqData]
    public async Task AddUserToRoleInnerAsync_ShouldReturnSuccess_WhenRoleExistAndAddSuccess(
        [Frozen] Mock<RoleManager<ApplicationRole>> roleManagerMock,
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        UserDomainService sut,
        string roleName,
        ApplicationUser user)
    {
        // Arrange
        roleManagerMock.Setup(roleManager => roleManager.RoleExistsAsync(roleName))
            .ReturnsAsync(true);
        userManagerMock.Setup(userManager => userManager.AddToRoleAsync(user, roleName))
            .ReturnsAsync(IdentityResult.Success);
        
        // Act
        var result = await sut.AddUserToRoleInnerAsync(user, roleName);
        
        // Assert
        result.Should().Be(RoleStatus.AddRoleToUserSuccess);
    }
    
    [Theory, AutoMoqData]
    public async Task AddUserToRoleInnerAsync_ShouldReturnFail_WhenRoleExistAndAddFailed(
        [Frozen] Mock<RoleManager<ApplicationRole>> roleManagerMock,
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        UserDomainService sut,
        string roleName,
        ApplicationUser user,
        IdentityError[] expectedErrors)
    {
        // Arrange
        roleManagerMock.Setup(roleManager => roleManager.RoleExistsAsync(roleName))
            .ReturnsAsync(true);
        userManagerMock.Setup(userManager => userManager.AddToRoleAsync(user, roleName))
            .ReturnsAsync(IdentityResult.Failed(expectedErrors));
        
        // Act
        var result = await sut.AddUserToRoleInnerAsync(user, roleName);
        
        // Assert
        result.Should().Be(RoleStatus.AddRoleToUserFailed);
    }

    [Theory, AutoMoqData]
    public async Task GetUserByPublicIdInnerAsync_ShouldReturnNull_WhenPublicIdIsInvalid(
        UserDomainService sut,
        string userPublicid)
    {
        // Arrange
        
        // Act
        var user = await sut.GetUserByPublicIdIncludeTenantAsync(userPublicid);
        
        // Assert
        user.Should().BeNull();
    }

    [Theory, AutoMoqData]
    public async Task GetUserByPublicIdInnerAsync_ShouldReturnNull_WhenPublicIdNotExist(
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        UserDomainService sut)
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();
        var user = new ApplicationUser()
        {
            Id = 1111,
            UserName = "TestUser",
            Email = "test@test.com"
        };
        var users = new List<ApplicationUser>(){user}.AsQueryable();
        var mockUsers = users.BuildMock();
        userManagerMock.Setup(x => x.Users).Returns(mockUsers);
        
        // Act
        var result = await sut.GetUserByPublicIdIncludeTenantAsync(expectedGuid.ToString());
        
        // Assert
        result.Should().BeNull();
    }
    
    [Theory, AutoMoqData]
    public async Task GetUserByPublicIdInnerAsync_ShouldReturnUser_WhenPublicIdExist(
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        UserDomainService sut)
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();
        var tenant = new Tenant { Id = 222, Name = "TestTenant" };
        var user = new ApplicationUser()
        {
            Id = 1111,
            UserName = "TestUser",
            Email = "test@test.com",
            PublicId = expectedGuid,
            Tenant = tenant,
            TenantId = 222
        };
        var users = new List<ApplicationUser>(){user}.AsQueryable();
        var mockUsers = users.BuildMock();
        userManagerMock.Setup(x => x.Users).Returns(mockUsers);
        
        // Act
        var result = await sut.GetUserByPublicIdIncludeTenantAsync(expectedGuid.ToString());
        
        // Assert
        result.Should().NotBeNull();
        result.PublicId.Should().Be(expectedGuid);
        result.Tenant.Should().NotBeNull();
    }
    
    [Theory, AutoMoqData]
    public async Task GetUserByPublicIdInnerAsync_ShouldReturnUserWithoutTenant_WhenTenantNotExist(
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        UserDomainService sut)
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();
        var user = new ApplicationUser()
        {
            Id = 1111,
            UserName = "TestUser",
            Email = "test@test.com",
            PublicId = expectedGuid,
        };
        var users = new List<ApplicationUser>(){user}.AsQueryable();
        var mockUsers = users.BuildMock();
        userManagerMock.Setup(x => x.Users).Returns(mockUsers);
        
        // Act
        var result = await sut.GetUserByPublicIdIncludeTenantAsync(expectedGuid.ToString());
        
        // Assert
        result.Should().NotBeNull();
        result.PublicId.Should().Be(expectedGuid);
        result.Tenant.Should().Be(null);
    }

    [Theory, AutoMoqData]
    public async Task ChangePasswordInnerAsync_ShouldReturnResult(
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        UserDomainService sut,
        string oldPassword,
        string newPassword,
        ApplicationUser user)
    {
        // Arrange
        userManagerMock.Setup(um => um.ChangePasswordAsync(user, oldPassword, newPassword))
            .ReturnsAsync(IdentityResult.Success);
        
        // Act
        var result = await sut.ChangePasswordInnerAsync(user, oldPassword, newPassword);
        
        // Assert
        result.Should().BeTrue();
    }

    [Theory, AutoMoqData]
    public async Task GetUserByEmailInnerAsync_ShouldReturnUser_WhenUserExist(
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        ApplicationUser user,
        UserDomainService sut)
    {
        userManagerMock.Setup(um => um.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        var result = await sut.GetUserByEmailInnerAsync(user.Email!);
        result.Should().NotBeNull();
    }
    
    [Theory, AutoMoqData]
    public async Task GetUserByEmailInnerAsync_ShouldReturnNull_WhenUserNotExist(
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        ApplicationUser user,
        UserDomainService sut)
    {
        userManagerMock.Setup(um => um.FindByEmailAsync(user.Email!)).ReturnsAsync(null as ApplicationUser);
        var result = await sut.GetUserByEmailInnerAsync(user.Email!);
        result.Should().BeNull();
    }
}