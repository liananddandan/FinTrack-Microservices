using AutoFixture.Xunit2;
using FluentAssertions;
using IdentityService.Common.Results;
using IdentityService.Common.Status;
using IdentityService.Domain.Entities;
using IdentityService.Services;
using IdentityService.Services.Interfaces;
using IdentityService.Tests.Attributes;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MockQueryable;
using Moq;
using SharedKernel.Common.Exceptions;
using SharedKernel.Common.Results;

namespace IdentityService.Tests.Services;

public class UserServiceTests
{
    [Theory, AutoMoqData]
    public async Task CreateUserInnerAsync_ShouldReturnUser_WhenCreateSuccess(
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        UserService sut,
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
        UserService sut,
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
        UserService sut,
        string roleName)
    {
        // Arrange
        roleManagerMock.Setup(roleManager => roleManager.RoleExistsAsync(roleName))
            .ReturnsAsync(true);
        
        // Act
        var result = await sut.CreateRoleInnerAsync(roleName);
        
        // Assert
        result.Should().Be(RoleStatus.RoleAlreadyExist);
    }

    [Theory, AutoMoqData]
    public async Task CreateRoleInnterAsync_ShouldReturnSuccess_WhenRoleNotExistAndCreateSuccess(
        [Frozen] Mock<RoleManager<ApplicationRole>> roleManagerMock,
        UserService sut,
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
        UserService sut,
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
        UserService sut,
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
        UserService sut,
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
        UserService sut,
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
    public async Task GetUserByIdAsync_ShouldReturnSuccess_WhenFindUser(
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        UserService sut,
        string userId)
    {
        // Arrange
        var expectedUser = new ApplicationUser
        {
            Id = 2002,
            UserName = "Mock User",
            Email = "mock@email.com",
            TenantId = 1111,
        };
        userManagerMock.Setup(userManager => userManager.FindByIdAsync(userId))
            .ReturnsAsync(expectedUser);
        
        // Act
        var result = await sut.GetUserByIdAsync(userId);
        
        // Assert
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(expectedUser.Id);
        result.Data.Email.Should().Be(expectedUser.Email);
        result.Data.TenantId.Should().Be(expectedUser.TenantId);
        result.Data.UserName.Should().Be(expectedUser.UserName);
        result.Data.PublicId.Should().NotBeEmpty();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserGetByIdSuccess);
        result.Message.Should().BeEquivalentTo("User Found By Id");
    }
    
    [Theory, AutoMoqData]
    public async Task GetUserByIdAsync_ShouldReturnFail_WhenNotFindUser(
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        UserService sut,
        string userId)
    {
        // Arrange
        userManagerMock.Setup(userManager => userManager.FindByIdAsync(userId))
            .ReturnsAsync(null as ApplicationUser);
        
        // Act
        var result = await sut.GetUserByIdAsync(userId);
        
        // Assert
        result.Data.Should().BeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserNotFound);
        result.Message.Should().BeEquivalentTo("User Not Found");
    }
    
    [Theory, AutoMoqData]
    public async Task ConfirmAccountAsync_ShouldReturnPublicInvalid_WhenPublicInvalid(
        UserService sut,
        string userId
    )
    {
        // Arrange
        
        // Act
        var result = await sut.ConfirmAccountEmailAsync(userId, It.IsAny<string>(), CancellationToken.None);
        
        // Assert
        result.Data.Should().BeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserInvalidPublicid);
    }

    [Theory, AutoMoqData]
    public async Task ConfirmAccountAsync_ShouldReturnUserNotFound_WhenUserNotExist(
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        UserService sut
        )
    {
        // Arrange
        var users = new List<ApplicationUser>().AsQueryable();
        var mockUsers = users.BuildMock();
        userManagerMock.Setup(x => x.Users).Returns(mockUsers);
        var publicId = Guid.NewGuid().ToString();
        
        // Act
        var result = await sut.ConfirmAccountEmailAsync(publicId, It.IsAny<string>(), CancellationToken.None);
        
        // Assert
        result.Data.Should().BeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserNotFound);
        result.Message.Should().BeEquivalentTo("User Not Found");
    }
    
    [Theory, AutoMoqData]
    public async Task ConfirmAccountAsync_ShouldReturnFail_WhenTokenVerifyFailed(
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        [Frozen] Mock<IUserVerificationService> userVerificationServiceMock,
        UserService sut,
        string token,
        ApplicationUser user
    )
    {
        // Arrange
        var users = new List<ApplicationUser>(){user}.AsQueryable();
        var mockUsers = users.BuildMock();
        userManagerMock.Setup(x => x.Users).Returns(mockUsers);
       
        userVerificationServiceMock.Setup(uv => 
            uv.ValidateTokenAsync(user, token, TokenPurpose.EmailConfirmation, CancellationToken.None))
            .ReturnsAsync(ServiceResult<bool>.Fail( It.IsAny<string>(), It.IsAny<string>()));
        
        // Act
        var result = await sut.ConfirmAccountEmailAsync(user.PublicId.ToString(), token, CancellationToken.None);
        
        // Assert
        result.Data.Should().BeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserEmailVerificationFailed);
        result.Message.Should().BeEquivalentTo("User Email Verification Failed");
    }
    
    [Theory, AutoMoqData]
    public async Task ConfirmAccountAsync_ShouldReturnSuccess_WhenTokenVerifySucceeded(
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        [Frozen] Mock<IUserVerificationService> userVerificationServiceMock,
        UserService sut,
        string token,
        ApplicationUser user)
    {
        // Arrange
        var users = new List<ApplicationUser>(){user}.AsQueryable();
        var mockUsers = users.BuildMock();
        userManagerMock.Setup(x => x.Users).Returns(mockUsers);
        userVerificationServiceMock.Setup(uv => 
                uv.ValidateTokenAsync(user, token, TokenPurpose.EmailConfirmation, CancellationToken.None))
            .ReturnsAsync(ServiceResult<bool>.Ok(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()));
        
        // Act
        var result = await sut.ConfirmAccountEmailAsync(user.PublicId.ToString(), token, CancellationToken.None);
        
        // Assert
        result.Data.Should().NotBeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserEmailVerificationSuccess);
        result.Message.Should().BeEquivalentTo("User Email Verification Success");
    }
}