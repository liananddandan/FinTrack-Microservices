using AutoFixture.Xunit2;
using FluentAssertions;
using IdentityService.Common.Status;
using IdentityService.Domain.Entities;
using IdentityService.Repositories.Interfaces;
using IdentityService.Services;
using IdentityService.Tests.Attributes;
using Microsoft.AspNetCore.Identity;
using MockQueryable;
using Moq;
using SharedKernel.Common.Constants;
using SharedKernel.Common.Exceptions;
using SharedKernel.Common.Results;
using StackExchange.Redis;

namespace IdentityService.Tests.Services;

public class UserDomainServiceTests
{
    [Theory, AutoMoqData]
    public async Task CreateUserInnerAsync_ShouldReturnUser_WhenCreateSuccess(
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        UserDomainService sut,
        string userName,
        string userEmail,
        long tenantId,
        long roleId)
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
        var (userResult, pwdResult) = await sut.CreateUserOrThrowInnerAsync(userName, userEmail, tenantId, roleId);
        
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
        long roleId,
        IdentityError[] expectedErrors)
    {
        // Arrange
        userManagerMock.Setup(usm =>
                usm.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(expectedErrors));
        
        // Act
        var ex = await Assert.ThrowsAsync<UserCreateException>(() =>
            sut.CreateUserOrThrowInnerAsync(userName, userEmail, tenantId, roleId));
        
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
            Email = "test@test.com",
            RoleId = 1
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
            TenantId = 222,
            RoleId = 1
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
            RoleId = 1
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
        var (result, reason) = await sut.ChangePasswordInnerAsync(user, oldPassword, newPassword);
        
        // Assert
        result.Should().BeTrue();
        reason.Should().NotBeNull();
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
    
    [Theory, AutoMoqData]
    public async Task ChangeFirstLoginStateInnerAsync_Should_Invoke_Repo_Method(
        [Frozen] Mock<IApplicationUserRepo> userRepoMock,
        ApplicationUser user,
        UserDomainService service)
    {
        // Act
        await service.ChangeFirstLoginStateInnerAsync(user);

        // Assert
        userRepoMock.Verify(r => r.ChangeFirstLoginStatus(user, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Theory, AutoMoqData]
    public async Task IncreaseUserJwtVersionInnerAsync_Should_Invoke_Repo_Method(
        [Frozen] Mock<IApplicationUserRepo> userRepoMock,
        [Frozen] Mock<IConnectionMultiplexer> redisMock,
        [Frozen] Mock<IDatabase> databaseMock,
        ApplicationUser user,
        UserDomainService service)
    {
        // Arrange
        databaseMock.Setup(db => db.StringSetAsync(
                $"{Constant.Redis.JwtVersionPrefix}{user.PublicId.ToString()}",
                user.JwtVersion.ToString(),
                TimeSpan.FromDays(30), false, When.Always, CommandFlags.None))
            .ReturnsAsync(true);
        redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), null))
            .Returns(databaseMock.Object);
        
        // Act
        await service.IncreaseUserJwtVersionInnerAsync(user);

        // Assert
        userRepoMock.Verify(r => r.IncreaseJwtVersion(user, It.IsAny<CancellationToken>()), Times.Once);
        databaseMock.Verify(db => db.StringSetAsync(
            $"{Constant.Redis.JwtVersionPrefix}{user.PublicId.ToString()}",
            user.JwtVersion.ToString(),
            TimeSpan.FromDays(30), false, When.Always, CommandFlags.None), Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task IsRoleExistAsync_ShouldReturnNotExist_WhenNotExist(
        [Frozen] Mock<RoleManager<ApplicationRole>> roleManagerMock,
        UserDomainService sut)
    {
        // Arrange
        var roleName = "TestRole";
        roleManagerMock.Setup(r => r.RoleExistsAsync(roleName)).ReturnsAsync(false);
        
        // Act
        var result = await sut.IsRoleExistAsync(roleName);
        
        // Assert
        result.Should().Be(RoleStatus.RoleNotExist);
        roleManagerMock.Verify(r => r.RoleExistsAsync(roleName), Times.Once);
    }
    
    [Theory, AutoMoqData]
    public async Task IsRoleExistAsync_ShouldReturnExist_WhenRoleExist(
        [Frozen] Mock<RoleManager<ApplicationRole>> roleManagerMock,
        UserDomainService sut)
    {
        // Arrange
        var roleName = "TestRole";
        roleManagerMock.Setup(r => r.RoleExistsAsync(roleName)).ReturnsAsync(true);
        
        // Act
        var result = await sut.IsRoleExistAsync(roleName);
        
        // Assert
        result.Should().Be(RoleStatus.RoleAlreadyExist);
        roleManagerMock.Verify(r => r.RoleExistsAsync(roleName), Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task CreateRoleInnerAsync_ShouldReturnExist_WhenRoleExist(
        [Frozen] Mock<RoleManager<ApplicationRole>> roleManagerMock,
        UserDomainService sut)
    {
        // Arrange
        var role = new ApplicationRole(){Name = "TestRole"};
        roleManagerMock.Setup(r => r.RoleExistsAsync(role.Name.ToUpperInvariant())).ReturnsAsync(true);
        
        // Act
        var result = await sut.CreateRoleInnerAsync(role);
        
        // Assert
        result.Should().Be(RoleStatus.RoleAlreadyExist);
        roleManagerMock.Verify(r => r.RoleExistsAsync(role.Name.ToUpperInvariant()), Times.Once);
    }
    
    [Theory, AutoMoqData]
    public async Task CreateRoleInnerAsync_ShouldReturnFailed_WhenRoleCreatedFail(
        [Frozen] Mock<RoleManager<ApplicationRole>> roleManagerMock,
        UserDomainService sut)
    {
        // Arrange
        var role = new ApplicationRole(){Name = "TestRole"};
        roleManagerMock.Setup(r => r.RoleExistsAsync(role.Name.ToUpperInvariant())).ReturnsAsync(false);
        roleManagerMock.Setup(r => r.CreateAsync(role)).ReturnsAsync(IdentityResult.Failed(new IdentityError()));
        // Act
        var result = await sut.CreateRoleInnerAsync(role);
        
        // Assert
        result.Should().Be(RoleStatus.CreateFailed);
        roleManagerMock.Verify(r => r.RoleExistsAsync(role.Name.ToUpperInvariant()), Times.Once);
        roleManagerMock.Verify(r => r.CreateAsync(role), Times.Once);
    }
    
    [Theory, AutoMoqData]
    public async Task CreateRoleInnerAsync_ShouldReturnSuccess_WhenRoleCreated(
        [Frozen] Mock<RoleManager<ApplicationRole>> roleManagerMock,
        UserDomainService sut)
    {
        // Arrange
        var role = new ApplicationRole(){Name = "TestRole"};
        roleManagerMock.Setup(r => r.RoleExistsAsync(role.Name.ToUpperInvariant())).ReturnsAsync(false);
        roleManagerMock.Setup(r => r.CreateAsync(role)).ReturnsAsync(IdentityResult.Success);
        // Act
        var result = await sut.CreateRoleInnerAsync(role);
        
        // Assert
        result.Should().Be(RoleStatus.CreateSuccess);
        roleManagerMock.Verify(r => r.RoleExistsAsync(role.Name.ToUpperInvariant()), Times.Once);
        roleManagerMock.Verify(r => r.CreateAsync(role), Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task GetRoleByNameInnerAsync_ShouldReturnNull_WhenRoleNotExist(
        [Frozen] Mock<RoleManager<ApplicationRole>> roleManagerMock,
        UserDomainService sut)
    {
        // Arrange
        var roles = new List<ApplicationRole>().AsQueryable();
        var rolesMock = roles.BuildMock();
        roleManagerMock.Setup(r => r.Roles).Returns(rolesMock);
        
        // Act
        var result = await sut.GetRoleByNameInnerAsync("TestRole");
        
        // Assert
        result.Should().BeNull();
    }
    
    [Theory, AutoMoqData]
    public async Task GetRoleByNameInnerAsync_ShouldReturnRole_WhenRoleExist(
        [Frozen] Mock<RoleManager<ApplicationRole>> roleManagerMock,
        UserDomainService sut)
    {
        // Arrange
        var role = new ApplicationRole(){ Name = "TestRole", NormalizedName = "TESTROLE"};
        var roles = new List<ApplicationRole>() {role}.AsQueryable();
        var rolesMock = roles.BuildMock();
        roleManagerMock.Setup(r => r.Roles).Returns(rolesMock);
        
        // Act
        var result = await sut.GetRoleByNameInnerAsync("TestRole");
        
        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(role.Name);
    }
    
    [Theory, AutoMoqData]
    public async Task GetAllUsersInTenantIncludeRoleAsync_ShouldReturnNull_WhenUsersNotExist(
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        UserDomainService sut)
    {
        // Arrange
        var users = new List<ApplicationUser>().AsQueryable();
        var usersMock = users.BuildMock();
        userManagerMock.Setup(u => u.Users).Returns(usersMock);
        
        // Act
        var result = await sut.GetAllUsersInTenantIncludeRoleAsync(1, CancellationToken.None);
        
        // Assert
        result = result.ToList();
        result.Should().NotBeNull();
        result.Count().Should().Be(0);
    }
    
    [Theory, AutoMoqData]
    public async Task GetAllUsersInTenantIncludeRoleAsync_ShouldReturnUsers_WhenUsersExist(
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        ApplicationUser user,
        UserDomainService sut)
    {
        // Arrange
        var users = new List<ApplicationUser>(){user}.AsQueryable();
        var usersMock = users.BuildMock();
        userManagerMock.Setup(u => u.Users).Returns(usersMock);
        
        // Act
        var result = await sut.GetAllUsersInTenantIncludeRoleAsync(user.TenantId, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
    }
}