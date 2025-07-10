using AutoFixture.Xunit2;
using FluentAssertions;
using IdentityService.Common.DTOs;
using IdentityService.Common.Results;
using IdentityService.Common.Status;
using IdentityService.Domain.Entities;
using IdentityService.Services;
using IdentityService.Services.Interfaces;
using IdentityService.Tests.Attributes;
using Microsoft.AspNetCore.Identity;
using MockQueryable;
using Moq;
using SharedKernel.Common.DTOs.Auth;
using SharedKernel.Common.Results;

namespace IdentityService.Tests.Services;

public class UserAppServiceTests
{
    [Theory, AutoMoqData]
    public async Task GetUserByIdAsync_ShouldReturnFail_WhenNotFindUser(
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        UserAppService sut,
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
        UserAppService sut,
        string userId
    )
    {
        // Arrange
        
        // Act
        var result = await sut.ConfirmAccountEmailAsync(userId, It.IsAny<string>(), CancellationToken.None);
        
        // Assert
        result.Data.Should().BeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserPublicIdInvalid);
    }

    [Theory, AutoMoqData]
    public async Task ConfirmAccountAsync_ShouldReturnUserNotFound_WhenUserNotExist(
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        UserAppService sut
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
        UserAppService sut,
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
        UserAppService sut,
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
    [Theory, AutoMoqData]
    public async Task GetUserByIdAsync_ShouldReturnSuccess_WhenFindUser(
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        UserAppService sut,
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
    public async Task UserLoginAsync_ShouldReturnFail_WhenUserNotFound(
        string email,
        string password,
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        UserAppService sut
        )
    {
        // Arrange
        var users = new List<ApplicationUser>().AsQueryable();
        var mockUsers = users.BuildMock();
        userManagerMock.Setup(x => x.Users).Returns(mockUsers);
        
        // Act
        var result = await sut.UserLoginAsync(email, password, CancellationToken.None);
        
        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserNotFound);
    }
    
    [Theory, AutoMoqData]
    public async Task UserLoginAsync_ShouldReturnFail_WhenEmailNotConfirmed(
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        UserAppService sut
    )
    {
        // Arrange
        string email = "test@test.com";
        string password = "password";
        var user = new ApplicationUser()
        {
            Id = 1111,
            UserName = "TestUser",
            Email = email,
            PublicId = Guid.NewGuid(),
            PasswordHash = password,
            EmailConfirmed = false
        };
        var users = new List<ApplicationUser>(){user}.AsQueryable();
        var mockUsers = users.BuildMock();
        userManagerMock.Setup(x => x.Users).Returns(mockUsers);
        
        // Act
        var result = await sut.UserLoginAsync(email, password, CancellationToken.None);
        
        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserEmailNotVerified);
    }
    
    [Theory, AutoMoqData]
    public async Task UserLoginAsync_ShouldReturnFail_WhenPasswordWrong(
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        UserAppService sut
    )
    {
        // Arrange
        string email = "test@test.com";
        string password = "password";
        var user = new ApplicationUser()
        {
            Id = 1111,
            UserName = "TestUser",
            Email = email,
            PublicId = Guid.NewGuid(),
            PasswordHash = password,
            EmailConfirmed = true
        };
        var users = new List<ApplicationUser>(){user}.AsQueryable();
        var mockUsers = users.BuildMock();
        userManagerMock.Setup(x => x.Users).Returns(mockUsers);
        userManagerMock.Setup(x => x.CheckPasswordAsync(user, password)).ReturnsAsync(false);
        // Act
        var result = await sut.UserLoginAsync(email, password, CancellationToken.None);
        
        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserEmailOrPasswordInvalid);
    }
    
    [Theory, AutoMoqData]
    public async Task UserLoginAsync_ShouldReturnFail_WhenRoleMissed(
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        UserAppService sut
    )
    {
        // Arrange
        string email = "test@test.com";
        string password = "password";
        var user = new ApplicationUser()
        {
            Id = 1111,
            UserName = "TestUser",
            Email = email,
            PublicId = Guid.NewGuid(),
            PasswordHash = password,
            EmailConfirmed = true
        };
        var users = new List<ApplicationUser>(){user}.AsQueryable();
        var mockUsers = users.BuildMock();
        userManagerMock.Setup(x => x.Users).Returns(mockUsers);
        userManagerMock.Setup(x => x.CheckPasswordAsync(user, password))
            .ReturnsAsync(true);
        userDomainServiceMock.Setup(x 
            => x.GetRoleInnerAsync(user, CancellationToken.None))
            .ReturnsAsync(null as string);
        // Act
        var result = await sut.UserLoginAsync(email, password, CancellationToken.None);
        
        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserCouldNotFindRole);
    }
    
    [Theory, AutoMoqData]
    public async Task UserLoginAsync_ShouldReturnSuccess_WhenFirstLoginIn(
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        UserAppService sut
    )
    {
        // Arrange
        string email = "test@test.com";
        string password = "password";
        var tenant = new Tenant { Id = 2222, Name = "TestTenant" };
        var user = new ApplicationUser()
        {
            Id = 1111,
            UserName = "TestUser",
            Email = email,
            PublicId = Guid.NewGuid(),
            PasswordHash = password,
            EmailConfirmed = true,
            Tenant = tenant
        };
        var roles = new List<string>(){"Admin_TestTenant"};
        var users = new List<ApplicationUser>(){user}.AsQueryable();
        var mockUsers = users.BuildMock();
        userManagerMock.Setup(x => x.Users).Returns(mockUsers);
        userManagerMock.Setup(x => x.CheckPasswordAsync(user, password))
            .ReturnsAsync(true);
        userDomainServiceMock.Setup(x 
                => x.GetRoleInnerAsync(user, CancellationToken.None))
            .ReturnsAsync("Admin_TestTenant");
        // Act
        var result = await sut.UserLoginAsync(email, password, CancellationToken.None);
        
        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserLoginSuccessButFirstLogin);
    }
    
    [Theory, AutoMoqData]
    public async Task UserLoginAsync_ShouldReturnSuccess_WhenLoginSuccess(
        [Frozen] Mock<UserManager<ApplicationUser>> userManagerMock,
        [Frozen] Mock<IJwtTokenService> jwtTokenServiceMock,
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        UserAppService sut,
        JwtTokenPair jwtTokenPair
    )
    {
        // Arrange
        string email = "test@test.com";
        string password = "password";
        var tenant = new Tenant { Id = 2222, Name = "TestTenant" };
        var user = new ApplicationUser()
        {
            Id = 1111,
            UserName = "TestUser",
            Email = email,
            PublicId = Guid.NewGuid(),
            PasswordHash = password,
            EmailConfirmed = true,
            Tenant = tenant,
            IsFirstLogin = false
        };
        var roles = new List<string>(){"Admin_TestTenant"};
        var users = new List<ApplicationUser>(){user}.AsQueryable();
        var mockUsers = users.BuildMock();
        userManagerMock.Setup(x => x.Users).Returns(mockUsers);
        userManagerMock.Setup(x => x.CheckPasswordAsync(user, password))
            .ReturnsAsync(true);
        userDomainServiceMock.Setup(x 
                => x.GetRoleInnerAsync(user, CancellationToken.None))
            .ReturnsAsync("Admin_TestTenant");        jwtTokenServiceMock.Setup(jts 
            => jts.GenerateJwtTokenPairAsync(It.IsAny<JwtClaimSource>()))
            .ReturnsAsync(ServiceResult<JwtTokenPair>.Ok(jwtTokenPair, "JwtTokenPair", "JwtTokenPair"));
        
        // Act
        var result = await sut.UserLoginAsync(email, password, CancellationToken.None);
        
        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserLoginSuccess);
    }
}