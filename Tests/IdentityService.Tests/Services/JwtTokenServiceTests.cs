using System.Security.Claims;
using AutoFixture.Xunit2;
using FluentAssertions;
using IdentityService.Common.DTOs;
using IdentityService.Common.Options;
using IdentityService.Common.Results;
using IdentityService.Common.Status;
using IdentityService.Domain.Entities;
using IdentityService.Services;
using IdentityService.Services.Interfaces;
using IdentityService.Tests.Attributes;
using Microsoft.Extensions.Options;
using Moq;

namespace IdentityService.Tests.Services;

public class JwtTokenServiceTests
{
    [Theory, AutoMoqData]
    public async Task GenerateJwtTokenAsync_ShouldReturnToken_WhenGenerateSuccess(
        [Frozen] Mock<IOptions<JwtOptions>> jwtOptionsMock,
        JwtTokenService sut,
        JwtClaimSource source,
        JwtTokenType tokenType
    )
    {
        // Arrange
        var jwtOptions = new JwtOptions()
        {
            Secret = "this-is-secret-very-long-token-value-and-long-token-value",
            Audience = "Audience",
            Issuer = "Issuer",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };
        jwtOptionsMock.Setup(j => j.Value).Returns(jwtOptions);

        // Act
        var result = await sut.GenerateJwtTokenAsync(source, tokenType);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNullOrEmpty();
        result.Code.Should().BeEquivalentTo(ResultCodes.Token.GenerateJwtTokenSuccess);
    }

    [Theory, AutoMoqData]
    public async Task GenerateJwtTokenPairAsync_ShouldReturnTokenPair_WhenGenerateSuccess(
        [Frozen] Mock<IOptions<JwtOptions>> jwtOptionsMock,
        JwtTokenService sut,
        JwtClaimSource source
    )
    {
        // Arrange
        var jwtOptions = new JwtOptions()
        {
            Secret = "this-is-secret-very-long-token-value-and-long-token-value",
            Audience = "Audience",
            Issuer = "Issuer",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };
        jwtOptionsMock.Setup(j => j.Value).Returns(jwtOptions);

        // Act
        var result = await sut.GenerateJwtTokenPairAsync(source);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.AccessToken.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
        result.Code.Should().BeEquivalentTo(ResultCodes.Token.GenerateJwtTokenPairSuccess);
    }

    [Theory, AutoMoqData]
    public async Task GetPrincipalFromTokenAsync_ShouldReturnNull_WhenTokenInvalid(
        JwtTokenService sut,
        [Frozen] Mock<IOptions<JwtOptions>> jwtOptionsMock,
        string jwtToken)
    {
        // Arrange
        var jwtOptions = new JwtOptions()
        {
            Secret = "this-is-secret-very-long-token-value-and-long-token-value",
            Audience = "Audience",
            Issuer = "Issuer",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };
        jwtOptionsMock.Setup(j => j.Value).Returns(jwtOptions);

        // Act
        var result = await sut.GetPrincipalFromTokenAsync(jwtToken);

        // Assert
        result.Data.Should().BeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.Token.JwtTokenInvalidForParse);
    }

    [Theory, AutoMoqData]
    public async Task GetPrincipalFromTokenAsync_ShouldReturnPrincipal_WhenTokenValid(
        JwtTokenService sut,
        [Frozen] Mock<IOptions<JwtOptions>> jwtOptionsMock)
    {
        // Arrange
        var jwtOptions = new JwtOptions()
        {
            Secret = "this-is-secret-very-long-token-value-and-long-token-value",
            Audience = "Audience",
            Issuer = "Issuer",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };
        jwtOptionsMock.Setup(j => j.Value).Returns(jwtOptions);
        var jwtClaimSource = new JwtClaimSource()
        {
            UserPublicId = new Guid("00000000-0000-0000-0000-000000000001").ToString(),
            JwtVersion = "1",
            TenantPublicId = new Guid("00000000-0000-0000-0000-000000000002").ToString(),
            UserRoleInTenant = "Role_test"
        };
        var jwtToken = await sut.GenerateJwtTokenAsync(jwtClaimSource, JwtTokenType.AccessToken);
        jwtToken.Should().NotBeNull();
        jwtToken.Data.Should().NotBeNullOrEmpty();

        // Act
        var result = await sut.GetPrincipalFromTokenAsync(jwtToken.Data);

        // Assert
        result.Data.Should().NotBeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.Token.JwtTokenParseSuccess);
        result.Data.Should().BeOfType<ClaimsPrincipal>();
        var principal = result.Data;
        principal.FindFirst(JwtTokenCustomKeys.UserPublicIdKey)!.Value.Should()
            .Be("00000000-0000-0000-0000-000000000001");
        principal.FindFirst(JwtTokenCustomKeys.JwtVersionKey)!.Value.Should().Be("1");
        principal.FindFirst(JwtTokenCustomKeys.TenantPublicIdKey)!.Value.Should()
            .Be("00000000-0000-0000-0000-000000000002");
    }

    [Theory, AutoMoqData]
    public async Task GenerateJwtTokenPairAsync_ShouldReturnFail_WhenTokenIsSInvalid(
        [Frozen] Mock<IOptions<JwtOptions>> jwtOptionsMock,
        JwtTokenService sut,
        string jwtToken)
    {
        // Arrange
        var jwtOptions = new JwtOptions()
        {
            Secret = "this-is-secret-very-long-token-value-and-long-token-value",
            Audience = "Audience",
            Issuer = "Issuer",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };
        jwtOptionsMock.Setup(j => j.Value).Returns(jwtOptions);

        // Act
        var result = await sut.RefreshJwtTokenPairAsync(jwtToken);

        // Assert
        result.Data.Should().BeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.Token.RefreshJwtTokenFailedTokenInvalid);
    }

    [Theory, AutoMoqData]
    public async Task GenerateJwtTokenPairAsync_ShouldReturnFail_WhenTokenTypeWrong(
        [Frozen] Mock<IOptions<JwtOptions>> jwtOptionsMock,
        JwtTokenService sut)
    {
        // Arrange
        var jwtOptions = new JwtOptions()
        {
            Secret = "this-is-secret-very-long-token-value-and-long-token-value",
            Audience = "Audience",
            Issuer = "Issuer",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };
        jwtOptionsMock.Setup(j => j.Value).Returns(jwtOptions);
        var jwtClaimSource = new JwtClaimSource()
        {
            UserPublicId = new Guid("00000000-0000-0000-0000-000000000001").ToString(),
            JwtVersion = "1",
            TenantPublicId = new Guid("00000000-0000-0000-0000-000000000002").ToString(),
            UserRoleInTenant = "Role_test"
        };
        var jwtToken = await sut.GenerateJwtTokenAsync(jwtClaimSource, JwtTokenType.AccessToken);
        jwtToken.Data.Should().NotBeNull();

        // Act
        var result = await sut.RefreshJwtTokenPairAsync(jwtToken.Data);

        // Assert
        result.Data.Should().BeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.Token.RefreshJwtTokenFailedTokenTypeInvalid);
    }

    [Theory, AutoMoqData]
    public async Task GenerateJwtTokenPairAsync_ShouldReturnFail_WhenTokenIdIsInvalid(
        [Frozen] Mock<IOptions<JwtOptions>> jwtOptionsMock,
        JwtTokenService sut)
    {
        // Arrange
        var jwtOptions = new JwtOptions()
        {
            Secret = "this-is-secret-very-long-token-value-and-long-token-value",
            Audience = "Audience",
            Issuer = "Issuer",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };
        jwtOptionsMock.Setup(j => j.Value).Returns(jwtOptions);
        var jwtClaimSource = new JwtClaimSource()
        {
            UserPublicId = new Guid("00000000-0000-0000-0000-000000000001").ToString(),
            JwtVersion = "",
            TenantPublicId = new Guid("00000000-0000-0000-0000-000000000002").ToString(),
            UserRoleInTenant = "Role_test"
        };
        var jwtToken = await sut.GenerateJwtTokenAsync(jwtClaimSource, JwtTokenType.RefreshToken);
        jwtToken.Data.Should().NotBeNull();

        // Act
        var result = await sut.RefreshJwtTokenPairAsync(jwtToken.Data);

        // Assert
        result.Data.Should().BeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.Token.RefreshJwtTokenFailedClaimMissing);
    }

    [Theory, AutoMoqData]
    public async Task GenerateJwtTokenPairAsync_ShouldReturnFail_WhenUserNotFound(
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        [Frozen] Mock<IOptions<JwtOptions>> jwtOptionsMock,
        JwtTokenService sut)
    {
        // Arrange
        var jwtOptions = new JwtOptions()
        {
            Secret = "this-is-secret-very-long-token-value-and-long-token-value",
            Audience = "Audience",
            Issuer = "Issuer",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };
        jwtOptionsMock.Setup(j => j.Value).Returns(jwtOptions);
        var jwtClaimSource = new JwtClaimSource()
        {
            UserPublicId = new Guid("00000000-0000-0000-0000-000000000001").ToString(),
            JwtVersion = "1",
            TenantPublicId = new Guid("00000000-0000-0000-0000-000000000002").ToString(),
            UserRoleInTenant = "Role_test"
        };
        var jwtToken = await sut.GenerateJwtTokenAsync(jwtClaimSource, JwtTokenType.RefreshToken);
        jwtToken.Data.Should().NotBeNull();
        userDomainServiceMock.Setup(us
                => us.GetUserByPublicIdInnerAsync(jwtClaimSource.UserPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as ApplicationUser);

        // Act
        var result = await sut.RefreshJwtTokenPairAsync(jwtToken.Data);

        // Assert
        result.Data.Should().BeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.Token.RefreshJwtTokenFailedClaimUserNotFound);
    }

    [Theory, AutoMoqData]
    public async Task GenerateJwtTokenPairAsync_ShouldReturnFail_WhenJwtVersionTooSmall(
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        [Frozen] Mock<IOptions<JwtOptions>> jwtOptionsMock,
        JwtTokenService sut)
    {
        // Arrange
        var jwtOptions = new JwtOptions()
        {
            Secret = "this-is-secret-very-long-token-value-and-long-token-value",
            Audience = "Audience",
            Issuer = "Issuer",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };
        jwtOptionsMock.Setup(j => j.Value).Returns(jwtOptions);
        var userPublicId = new Guid("00000000-0000-0000-0000-000000000001");
        var tenantPublicId = new Guid("00000000-0000-0000-0000-000000000002");
        var jwtClaimSource = new JwtClaimSource()
        {
            UserPublicId = userPublicId.ToString(),
            JwtVersion = "1",
            TenantPublicId = tenantPublicId.ToString(),
            UserRoleInTenant = "Role_test"
        };
        var jwtToken = await sut.GenerateJwtTokenAsync(jwtClaimSource, JwtTokenType.RefreshToken);
        jwtToken.Data.Should().NotBeNull();

        var tenant = new Tenant()
        {
            Id = 4444,
            PublicId = tenantPublicId
        };
        var user = new ApplicationUser()
        {
            PublicId = userPublicId,
            UserName = "TestUser",
            Email = "test@test.com",
            Tenant = tenant,
            TenantId = 4444,
            JwtVersion = 3
        };
        userDomainServiceMock.Setup(us
                => us.GetUserByPublicIdInnerAsync(jwtClaimSource.UserPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await sut.RefreshJwtTokenPairAsync(jwtToken.Data);

        // Assert
        result.Data.Should().BeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.Token.RefreshJwtTokenFailedVersionInvalid);
    }

    [Theory, AutoMoqData]
    public async Task GenerateJwtTokenPairAsync_ShouldReturnFail_WhenTenantIdIsInvalid(
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        [Frozen] Mock<IOptions<JwtOptions>> jwtOptionsMock,
        JwtTokenService sut)
    {
        // Arrange
        var jwtOptions = new JwtOptions()
        {
            Secret = "this-is-secret-very-long-token-value-and-long-token-value",
            Audience = "Audience",
            Issuer = "Issuer",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };
        jwtOptionsMock.Setup(j => j.Value).Returns(jwtOptions);
        var userPublicId = new Guid("00000000-0000-0000-0000-000000000001");
        var tenantPublicId = new Guid("00000000-0000-0000-0000-000000000002");
        var jwtClaimSource = new JwtClaimSource()
        {
            UserPublicId = userPublicId.ToString(),
            JwtVersion = "3",
            TenantPublicId = tenantPublicId.ToString(),
            UserRoleInTenant = "Role_test"
        };
        var jwtToken = await sut.GenerateJwtTokenAsync(jwtClaimSource, JwtTokenType.RefreshToken);
        jwtToken.Data.Should().NotBeNull();

        var tenant = new Tenant()
        {
            Id = 4444,
            PublicId = new Guid("00000000-0000-0000-0000-000000000010"),
        };
        var user = new ApplicationUser()
        {
            PublicId = userPublicId,
            UserName = "TestUser",
            Email = "test@test.com",
            Tenant = tenant,
            TenantId = 4444,
            JwtVersion = 3
        };
        userDomainServiceMock.Setup(us
                => us.GetUserByPublicIdInnerAsync(jwtClaimSource.UserPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await sut.RefreshJwtTokenPairAsync(jwtToken.Data);

        // Assert
        result.Data.Should().BeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.Token.RefreshJwtTokenFailedClaimTenantIdInvalid);
    }

    [Theory, AutoMoqData]
    public async Task GenerateJwtTokenPairAsync_ShouldReturnFail_WhenRoleIsInvalid(
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        [Frozen] Mock<IOptions<JwtOptions>> jwtOptionsMock,
        JwtTokenService sut)
    {
        // Arrange
        var jwtOptions = new JwtOptions()
        {
            Secret = "this-is-secret-very-long-token-value-and-long-token-value",
            Audience = "Audience",
            Issuer = "Issuer",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };
        jwtOptionsMock.Setup(j => j.Value).Returns(jwtOptions);
        var userPublicId = new Guid("00000000-0000-0000-0000-000000000001");
        var tenantPublicId = new Guid("00000000-0000-0000-0000-000000000002");
        var jwtClaimSource = new JwtClaimSource()
        {
            UserPublicId = userPublicId.ToString(),
            JwtVersion = "3",
            TenantPublicId = tenantPublicId.ToString(),
            UserRoleInTenant = "Role_test"
        };
        var jwtToken = await sut.GenerateJwtTokenAsync(jwtClaimSource, JwtTokenType.RefreshToken);
        jwtToken.Data.Should().NotBeNull();

        var tenant = new Tenant()
        {
            Id = 4444,
            PublicId = tenantPublicId,
        };
        var user = new ApplicationUser()
        {
            PublicId = userPublicId,
            UserName = "TestUser",
            Email = "test@test.com",
            Tenant = tenant,
            TenantId = 4444,
            JwtVersion = 3
        };
        userDomainServiceMock.Setup(us
                => us.GetUserByPublicIdInnerAsync(jwtClaimSource.UserPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        userDomainServiceMock.Setup(us
                => us.GetRoleInnerAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as string);

        // Act
        var result = await sut.RefreshJwtTokenPairAsync(jwtToken.Data);

        // Assert
        result.Data.Should().BeNull();
        result.Code.Should().BeEquivalentTo(ResultCodes.Token.RefreshJwtTokenFailedRoleInvalid);
    }

    [Theory, AutoMoqData]
    public async Task GenerateJwtTokenPairAsync_ShouldReturnSuccess_WhenRefreshTokenIsValid(
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        [Frozen] Mock<IOptions<JwtOptions>> jwtOptionsMock,
        JwtTokenService sut)
    {
        // Arrange
        var jwtOptions = new JwtOptions()
        {
            Secret = "this-is-secret-very-long-token-value-and-long-token-value",
            Audience = "Audience",
            Issuer = "Issuer",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };
        jwtOptionsMock.Setup(j => j.Value).Returns(jwtOptions);
        var userPublicId = new Guid("00000000-0000-0000-0000-000000000001");
        var tenantPublicId = new Guid("00000000-0000-0000-0000-000000000002");
        var jwtClaimSource = new JwtClaimSource()
        {
            UserPublicId = userPublicId.ToString(),
            JwtVersion = "3",
            TenantPublicId = tenantPublicId.ToString(),
            UserRoleInTenant = "Role_test"
        };
        var jwtToken = await sut.GenerateJwtTokenAsync(jwtClaimSource, JwtTokenType.RefreshToken);
        jwtToken.Data.Should().NotBeNull();

        var tenant = new Tenant()
        {
            Id = 4444,
            PublicId = tenantPublicId,
        };
        var user = new ApplicationUser()
        {
            PublicId = userPublicId,
            UserName = "TestUser",
            Email = "test@test.com",
            Tenant = tenant,
            TenantId = 4444,
            JwtVersion = 3
        };
        userDomainServiceMock.Setup(us
                => us.GetUserByPublicIdInnerAsync(jwtClaimSource.UserPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        userDomainServiceMock.Setup(us
                => us.GetRoleInnerAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync("AdminRole");

        // Act
        var result = await sut.RefreshJwtTokenPairAsync(jwtToken.Data);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.AccessToken.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
        result.Code.Should().BeEquivalentTo(ResultCodes.Token.RefreshJwtTokenSuccess);
    }
}