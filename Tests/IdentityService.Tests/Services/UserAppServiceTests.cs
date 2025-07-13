using AutoFixture.Xunit2;
using FluentAssertions;
using IdentityService.Common.DTOs;
using IdentityService.Common.Results;
using IdentityService.Common.Status;
using IdentityService.Domain.Entities;
using IdentityService.Repositories.Interfaces;
using IdentityService.Services;
using IdentityService.Services.Interfaces;
using IdentityService.Tests.Attributes;
using IdentityService.Tests.Extensions;
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

    [Theory, AutoMoqData]
    public async Task SetUserPasswordAsync_ShouldReturnFalse_WhenUserNotFound(
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        UserAppService sut,
        string userPublicId,
        string jwtVersion,
        string oldPassword,
        string newPassword)
    {
        // Arrange
        userDomainServiceMock.Setup(uds => uds.GetUserByPublicIdIncludeTenantAsync(userPublicId, CancellationToken.None))
            .ReturnsAsync(null as ApplicationUser);
        
        // Act
        var result = await sut.SetUserPasswordAsync(userPublicId, jwtVersion, oldPassword, newPassword, false);
        
        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserNotFound);
    }
    
    [Theory, AutoMoqData]
    public async Task SetUserPasswordAsync_ShouldReturnFalse_WhenJwtVersionParseToLongFail(
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        UserAppService sut,
        string userPublicId,
        string oldPassword,
        string newPassword)
    {
        // Arrange
        string jwtVersion = "sss";
        var user = new ApplicationUser()
        {
            JwtVersion = 3
        };
        userDomainServiceMock.Setup(uds => uds.GetUserByPublicIdIncludeTenantAsync(userPublicId, CancellationToken.None))
            .ReturnsAsync(user);
        
        // Act
        var result = await sut.SetUserPasswordAsync(userPublicId, jwtVersion, oldPassword, newPassword, false);
        
        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.Token.JwtTokenVersionInvalid);
    }
    
    [Theory, AutoMoqData]
    public async Task SetUserPasswordAsync_ShouldReturnFalse_WhenJwtVersionMismatch(
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        UserAppService sut,
        string userPublicId,
        string oldPassword,
        string newPassword)
    {
        // Arrange
        string jwtVersion = "1";
        var user = new ApplicationUser()
        {
            JwtVersion = 3
        };
        userDomainServiceMock.Setup(uds => uds.GetUserByPublicIdIncludeTenantAsync(userPublicId, CancellationToken.None))
            .ReturnsAsync(user);
        
        // Act
        var result = await sut.SetUserPasswordAsync(userPublicId, jwtVersion, oldPassword, newPassword, false);
        
        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.Token.JwtTokenVersionInvalid);
    }
    
    [Theory, AutoMoqData]
    public async Task SetUserPasswordAsync_ShouldReturnFail_WhenChangePasswordWithoutSet(
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        UserAppService sut,
        string userPublicId,
        string oldPassword,
        string newPassword)
    {
        // Arrange
        unitOfWorkMock.SetupExecuteWithTransaction<bool>();
        string jwtVersion = "3";
        var user = new ApplicationUser()
        {
            JwtVersion = 3,
            IsFirstLogin = true
        };
        userDomainServiceMock.Setup(uds => uds.GetUserByPublicIdIncludeTenantAsync(userPublicId, CancellationToken.None))
            .ReturnsAsync(user);
        userDomainServiceMock.Setup(uds => uds.ChangePasswordInnerAsync(user, oldPassword, newPassword, CancellationToken.None))
            .ReturnsAsync(true);
        
        // Act
        var result = await sut.SetUserPasswordAsync(userPublicId, jwtVersion, 
            oldPassword, newPassword, true);
        
        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserResetPasswordBeforeSetPasswordFailed);
    }
    
    [Theory, AutoMoqData]
    public async Task SetUserPasswordAsync_ShouldReturnSuccess_WhenResetPasswordSuccess(
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        UserAppService sut,
        string userPublicId,
        string oldPassword,
        string newPassword)
    {
        // Arrange
        unitOfWorkMock.SetupExecuteWithTransaction<bool>();
        string jwtVersion = "3";
        var user = new ApplicationUser()
        {
            JwtVersion = 3,
            IsFirstLogin = false
        };
        userDomainServiceMock
            .Setup(uds => uds.GetUserByPublicIdIncludeTenantAsync(userPublicId, CancellationToken.None))
            .ReturnsAsync(user);
        userDomainServiceMock
            .Setup(uds => uds.ChangePasswordInnerAsync(user, oldPassword, newPassword, CancellationToken.None))
            .ReturnsAsync(true);
        
        // Act
        var result = await sut.SetUserPasswordAsync(userPublicId, jwtVersion, 
            oldPassword, newPassword, true);
        
        // Assert
        result.Success.Should().BeTrue();
        userDomainServiceMock.Verify(uds => uds.ChangeFirstLoginStateInnerAsync(user, CancellationToken.None), Times.Never);
        userDomainServiceMock.Verify(uds => uds.IncreaseUserJwtVersionInnerAsync(user, CancellationToken.None), Times.Once);
    }
    
    [Theory, AutoMoqData]
    public async Task SetUserPasswordAsync_ShouldReturnTrue_WhenSetPasswordSuccess(
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        UserAppService sut,
        string userPublicId,
        string oldPassword,
        string newPassword)
    {
        // Arrange
        unitOfWorkMock.SetupExecuteWithTransaction<bool>();
        string jwtVersion = "3";
        var user = new ApplicationUser()
        {
            JwtVersion = 3
        };
        userDomainServiceMock.Setup(uds => uds.GetUserByPublicIdIncludeTenantAsync(userPublicId, CancellationToken.None))
            .ReturnsAsync(user);
        userDomainServiceMock.Setup(uds => uds.ChangePasswordInnerAsync(user, oldPassword, newPassword, CancellationToken.None))
            .ReturnsAsync(true);
        
        // Act
        var result = await sut.SetUserPasswordAsync(userPublicId, jwtVersion, oldPassword, newPassword, false);
        
        // Assert
        result.Success.Should().BeTrue();
        userDomainServiceMock.Verify(uds => uds.ChangeFirstLoginStateInnerAsync(user, CancellationToken.None), Times.Once);
        userDomainServiceMock.Verify(uds => uds.IncreaseUserJwtVersionInnerAsync(user, CancellationToken.None), Times.Once);
    }
    
    [Theory, AutoMoqData]
    public async Task RefreshUserTokenPairAsync_ShouldReturnFail_WhenUserIdWrong(
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        UserAppService sut,
        string userPublicId,
        string jwtVersion)
    {
        // Arrange
        userDomainServiceMock
            .Setup(uds => uds.GetUserByPublicIdIncludeTenantAsync(userPublicId, CancellationToken.None))
            .ReturnsAsync((ApplicationUser?)null);
        
        // Act
        var result = await sut.RefreshUserTokenPairAsync(userPublicId, jwtVersion, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserNotFound);
    }
    
    [Theory, AutoMoqData]
    public async Task RefreshUserTokenPairAsync_ShouldReturnFail_WhenJwtVersionInvalid(
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        UserAppService sut,
        string userPublicId,
        string jwtVersion,
        ApplicationUser user)
    {
        // Arrange
        userDomainServiceMock
            .Setup(uds => uds.GetUserByPublicIdIncludeTenantAsync(userPublicId, CancellationToken.None))
            .ReturnsAsync(user);
        
        // Act
        var result = await sut.RefreshUserTokenPairAsync(userPublicId, jwtVersion, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.Token.JwtTokenVersionInvalid);
    }
    
    [Theory, AutoMoqData]
    public async Task RefreshUserTokenPairAsync_ShouldReturnFail_WhenJwtVersionMismatch(
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        UserAppService sut,
        string userPublicId,
        ApplicationUser user)
    {
        // Arrange
        string jwtVersion = "1";
        user.JwtVersion = 3;
        userDomainServiceMock
            .Setup(uds => uds.GetUserByPublicIdIncludeTenantAsync(userPublicId, CancellationToken.None))
            .ReturnsAsync(user);
        
        // Act
        var result = await sut.RefreshUserTokenPairAsync(userPublicId, jwtVersion, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.Token.JwtTokenVersionInvalid);
    }
    
    [Theory, AutoMoqData]
    public async Task RefreshUserTokenPairAsync_ShouldReturnFail_WhenUserNoRole(
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        UserAppService sut,
        string userPublicId,
        ApplicationUser user)
    {
        // Arrange
        var jwtVersion = "1";
        user.JwtVersion = 1;
        userDomainServiceMock
            .Setup(uds => uds.GetUserByPublicIdIncludeTenantAsync(userPublicId, CancellationToken.None))
            .ReturnsAsync(user);
        userDomainServiceMock
            .Setup(uds => uds.GetRoleInnerAsync(user, CancellationToken.None))
            .ReturnsAsync((string?)null);
        
        // Act
        var result = await sut.RefreshUserTokenPairAsync(userPublicId, jwtVersion, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserCouldNotFindRole);
    }
    
    [Theory, AutoMoqData]
    public async Task RefreshUserTokenPairAsync_ShouldReturnSuccess_WhenGenerateSuccess(
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        [Frozen] Mock<IJwtTokenService> jwtTokenServiceMock,
        UserAppService sut,
        string userPublicId,
        ApplicationUser user,
        JwtTokenPair jwtTokenPair
        )
    {
        // Arrange
        var jwtVersion = "1";
        user.JwtVersion = 1;
        userDomainServiceMock
            .Setup(uds => uds.GetUserByPublicIdIncludeTenantAsync(userPublicId, CancellationToken.None))
            .ReturnsAsync(user);
        userDomainServiceMock
            .Setup(uds => uds.GetRoleInnerAsync(user, CancellationToken.None))
            .ReturnsAsync("Admin_test");
        jwtTokenServiceMock
            .Setup(jts => jts.GenerateJwtTokenPairAsync(It.IsAny<JwtClaimSource>()))
            .ReturnsAsync(ServiceResult<JwtTokenPair>.Ok(jwtTokenPair, "jwt token"));
        
        // Act
        var result = await sut.RefreshUserTokenPairAsync(userPublicId, jwtVersion, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserRefreshTokenSuccess);
    }

    [Theory, AutoMoqData]
    public async Task GetUserInfoAsync_ShouldReturnFail_WhenTenantMissing(
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        UserAppService sut, 
        ApplicationUser user,
        string userPublicId)
    {
        // Arrange
        user.Tenant = null;
        userDomainServiceMock.Setup(
            uds => uds.GetUserByPublicIdIncludeTenantAsync(userPublicId, CancellationToken.None))
            .ReturnsAsync(user);
        
        // Act
        var result = await sut.GetUserInfoAsync(userPublicId, user.JwtVersion.ToString(), CancellationToken.None);
        
        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserTenantInfoMissed);
    }
    
    [Theory, AutoMoqData]
    public async Task GetUserInfoAsync_ShouldReturnFail_WhenRoleMissing(
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        UserAppService sut, 
        ApplicationUser user,
        string userPublicId)
    {
        // Arrange
        userDomainServiceMock.Setup(
                uds => uds.GetUserByPublicIdIncludeTenantAsync(userPublicId, CancellationToken.None))
            .ReturnsAsync(user);
        userDomainServiceMock.Setup(uds => uds.GetRoleInnerAsync(user, CancellationToken.None))
            .ReturnsAsync((string?)null);
        
        // Act
        var result = await sut.GetUserInfoAsync(userPublicId, user.JwtVersion.ToString(), CancellationToken.None);
        
        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserCouldNotFindRole);
    }
    
    [Theory, AutoMoqData]
    public async Task GetUserInfoAsync_ShouldReturnSuccess_WhenGetInfo(
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        UserAppService sut, 
        ApplicationUser user,
        string userPublicId)
    {
        // Arrange
        userDomainServiceMock.Setup(
                uds => uds.GetUserByPublicIdIncludeTenantAsync(userPublicId, CancellationToken.None))
            .ReturnsAsync(user);
        userDomainServiceMock.Setup(uds => uds.GetRoleInnerAsync(user, CancellationToken.None))
            .ReturnsAsync("Admin_test");
        
        // Act
        var result = await sut.GetUserInfoAsync(userPublicId, user.JwtVersion.ToString(), CancellationToken.None);
        
        // Assert
        result.Success.Should().BeTrue();
        result.Code.Should().BeEquivalentTo(ResultCodes.User.UserGetInfoSuccess);
        var userInfo = result.Data;
        userInfo.Should().NotBeNull();
        userInfo.userPublicId.Should().Be(user.PublicId.ToString());
        userInfo.email.Should().Be(user.Email);
        userInfo.userName.Should().Be(user.UserName);
        userInfo.roleName.Should().Be("Admin_test");
        var tenantInfo = userInfo.tenantInfoDto;
        tenantInfo.Should().NotBeNull();
        tenantInfo.TenantPublicId.Should().Be(user.Tenant!.PublicId.ToString());
        tenantInfo.TenantName.Should().Be(user.Tenant!.Name);
    }
}