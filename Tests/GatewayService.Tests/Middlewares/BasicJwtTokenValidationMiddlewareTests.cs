using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoFixture.Xunit2;
using FluentAssertions;
using GatewayService.Common.Options;
using GatewayService.Middlewares;
using GatewayService.Tests.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using SharedKernel.Common.Options;
using StackExchange.Redis;

namespace GatewayService.Tests.Middlewares;

public class BasicJwtTokenValidationMiddlewareTests
{

    [Theory, AutoMoqData]
    public async Task Invoke_WithWhiteList_CallsNextMiddleware(
        [Frozen] Mock<IConnectionMultiplexer> redisMock,
        [Frozen] Mock<IDatabase> dbMock)
    {
        // arrange
        dbMock.Setup(db => db.StringGetAsync(
                It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync("3");

        redisMock.Setup(c => c.GetDatabase(It.IsAny<int>(), null))
            .Returns(dbMock.Object);
        
        var httpContext = new DefaultHttpContext();
        var wasCalled = false;
        var next = new RequestDelegate((_) =>
        {
            wasCalled = true;
            return Task.CompletedTask;
        });

        var authOptions = new AuthenticationOptions()
        {
            AuthWhiteList = new List<string>() {     
                "/api/account/confirm-email",
                "/api/account/login",
                "/api/tenant/register" }
        };
        var authWrapper = new OptionsWrapper<AuthenticationOptions>(authOptions);
        
        var middleware = new BasicJwtTokenValidationMiddleware(next, authWrapper, redisMock.Object);
        
        httpContext.Request.Path = "/api/account/confirm-email";
        
        // action
        await middleware.Invoke(httpContext);
        wasCalled.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(200);
    }

    [Theory, AutoMoqData]
    public async Task Invoke_WithNullJwtToken_CallsNextMiddleware(
        [Frozen] Mock<IConnectionMultiplexer> redisMock,
        [Frozen] Mock<IDatabase> dbMock)
    {
        // Arrange
        dbMock.Setup(db => db.StringGetAsync(
                It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync("3");

        redisMock.Setup(c => c.GetDatabase(It.IsAny<int>(), null))
            .Returns(dbMock.Object);
        var httpContext = new DefaultHttpContext();
        var wasCalled = false;
        var next = new RequestDelegate((_) =>
        {
            wasCalled = true;
            return Task.CompletedTask;
        });
        var jwtOptions = new JwtOptions()
        {
            Secret = "very-strong-secret-key-for-hmac256",
            Issuer = "FinTrack",
            Audience = "FinTrackAudience",
            AccessTokenExpirationMinutes = 60,
            FirstLoginChangePasswordExpirationMinutes = 10,
            RefreshTokenExpirationDays = 7,
            InvitationTokenExpirationDays = 7
        };
        var jwtWrapper = new OptionsWrapper<JwtOptions>(jwtOptions);

        var authOptions = new AuthenticationOptions()
        {
            AuthWhiteList = new List<string>() {     
                "/api/account/confirm-email",
                "/api/account/login",
                "/api/tenant/register" }
        };
        var authWrapper = new OptionsWrapper<AuthenticationOptions>(authOptions);
        
        var middleware = new BasicJwtTokenValidationMiddleware(next, authWrapper, redisMock.Object);

        // action
        await middleware.Invoke(httpContext);

        // assert
        wasCalled.Should().BeFalse();
        httpContext.Response.StatusCode.Should().Be(401);
    }

    [Theory, AutoMoqData]
    public async Task Invoke_WithEmptyJwtToken_CallsNextMiddleware(
        [Frozen] Mock<IConnectionMultiplexer> redisMock,
        [Frozen] Mock<IDatabase> dbMock)
    {
        // Arrange
        dbMock.Setup(db => db.StringGetAsync(
                It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync("3");

        redisMock.Setup(c => c.GetDatabase(It.IsAny<int>(), null))
            .Returns(dbMock.Object);
        
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Append("Authorization", string.Empty);
        var wasCalled = false;
        var next = new RequestDelegate((_) =>
        {
            wasCalled = true;
            return Task.CompletedTask;
        });
        var jwtOptions = new JwtOptions()
        {
            Secret = "very-strong-secret-key-for-hmac256",
            Issuer = "FinTrack",
            Audience = "FinTrackAudience",
            AccessTokenExpirationMinutes = 60,
            FirstLoginChangePasswordExpirationMinutes = 10,
            RefreshTokenExpirationDays = 7,
            InvitationTokenExpirationDays = 7
        };
        var jwtWrapper = new OptionsWrapper<JwtOptions>(jwtOptions);
        var authOptions = new AuthenticationOptions()
        {
            AuthWhiteList = new List<string>() {     
                "/api/account/confirm-email",
                "/api/account/login",
                "/api/tenant/register" }
        };
        var authWrapper = new OptionsWrapper<AuthenticationOptions>(authOptions);
        
        var middleware = new BasicJwtTokenValidationMiddleware(next, authWrapper, redisMock.Object);

        // action
        await middleware.Invoke(httpContext);

        // assert
        wasCalled.Should().BeFalse();
        httpContext.Response.StatusCode.Should().Be(401);
    }

    [Theory, AutoMoqData]
    public async Task Invoke_WithInviteJwtToken_CallsNextMiddleware(
        [Frozen] Mock<IConnectionMultiplexer> redisMock,
        [Frozen] Mock<IDatabase> dbMock)
    {
        // Arrange
        dbMock.Setup(db => db.StringGetAsync(
                It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync("3");

        redisMock.Setup(c => c.GetDatabase(It.IsAny<int>(), null))
            .Returns(dbMock.Object);
        
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Append("Authorization", "Invite ThisIsAInviteJwtToken");
        var wasCalled = false;
        var next = new RequestDelegate((_) =>
        {
            wasCalled = true;
            return Task.CompletedTask;
        });
        var jwtOptions = new JwtOptions()
        {
            Secret = "very-strong-secret-key-for-hmac256",
            Issuer = "FinTrack",
            Audience = "FinTrackAudience",
            AccessTokenExpirationMinutes = 60,
            FirstLoginChangePasswordExpirationMinutes = 10,
            RefreshTokenExpirationDays = 7,
            InvitationTokenExpirationDays = 7
        };
        var jwtWrapper = new OptionsWrapper<JwtOptions>(jwtOptions);
        var authOptions = new AuthenticationOptions()
        {
            AuthWhiteList = new List<string>() {     
                "/api/account/confirm-email",
                "/api/account/login",
                "/api/tenant/register" }
        };
        var authWrapper = new OptionsWrapper<AuthenticationOptions>(authOptions);
        
        var middleware = new BasicJwtTokenValidationMiddleware(next, authWrapper, redisMock.Object);

        // action
        await middleware.Invoke(httpContext);

        // assert
        wasCalled.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(200);
    }

    [Theory, AutoMoqData]
    public async Task Invoke_WithInvalidBearerJwtToken_ReturnUnauthorized(
        [Frozen] Mock<IConnectionMultiplexer> redisMock,
        [Frozen] Mock<IDatabase> dbMock)
    {
        // Arrange
        dbMock.Setup(db => db.StringGetAsync(
                It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync("3");

        redisMock.Setup(c => c.GetDatabase(It.IsAny<int>(), null))
            .Returns(dbMock.Object);
        
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Append("Authorization", "Bearer ThisIsAInvalidBearerJwtToken");
        var wasCalled = false;
        var next = new RequestDelegate((_) =>
        {
            wasCalled = true;
            return Task.CompletedTask;
        });
        var jwtOptions = new JwtOptions()
        {
            Secret = "very-strong-secret-key-for-hmac256",
            Issuer = "FinTrack",
            Audience = "FinTrackAudience",
            AccessTokenExpirationMinutes = 60,
            FirstLoginChangePasswordExpirationMinutes = 10,
            RefreshTokenExpirationDays = 7,
            InvitationTokenExpirationDays = 7
        };
        var jwtWrapper = new OptionsWrapper<JwtOptions>(jwtOptions);
        var authOptions = new AuthenticationOptions()
        {
            AuthWhiteList = new List<string>() {     
                "/api/account/confirm-email",
                "/api/account/login",
                "/api/tenant/register" }
        };
        var authWrapper = new OptionsWrapper<AuthenticationOptions>(authOptions);
        
        var middleware = new BasicJwtTokenValidationMiddleware(next, authWrapper, redisMock.Object);

        // action
        await middleware.Invoke(httpContext);

        // assert
        wasCalled.Should().BeFalse();
        httpContext.Response.StatusCode.Should().Be(401);
    }
    
    [Theory, AutoMoqData]
    public async Task Invoke_WithInvalidJwtTokenType_ReturnUnauthorized(
        [Frozen] Mock<IConnectionMultiplexer> redisMock,
        [Frozen] Mock<IDatabase> dbMock)
    {
        // Arrange
        dbMock.Setup(db => db.StringGetAsync(
                It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync("3");

        redisMock.Setup(c => c.GetDatabase(It.IsAny<int>(), null))
            .Returns(dbMock.Object);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Append("Authorization", "Customized ThisIsACustomizedJwtToken");
        var wasCalled = false;
        var next = new RequestDelegate((_) =>
        {
            wasCalled = true;
            return Task.CompletedTask;
        });
        var jwtOptions = new JwtOptions()
        {
            Secret = "very-strong-secret-key-for-hmac256",
            Issuer = "FinTrack",
            Audience = "FinTrackAudience",
            AccessTokenExpirationMinutes = 60,
            FirstLoginChangePasswordExpirationMinutes = 10,
            RefreshTokenExpirationDays = 7,
            InvitationTokenExpirationDays = 7
        };
        var jwtWrapper = new OptionsWrapper<JwtOptions>(jwtOptions);
        var authOptions = new AuthenticationOptions()
        {
            AuthWhiteList = new List<string>() {     
                "/api/account/confirm-email",
                "/api/account/login",
                "/api/tenant/register" }
        };
        var authWrapper = new OptionsWrapper<AuthenticationOptions>(authOptions);
        
        var middleware = new BasicJwtTokenValidationMiddleware(next, authWrapper, redisMock.Object);
        // action
        await middleware.Invoke(httpContext);

        // assert
        wasCalled.Should().BeFalse();
        httpContext.Response.StatusCode.Should().Be(401);
    }

        [Theory, AutoMoqData]
    public async Task Invoke_WithInvalidJwtTokenVersion_CallsNextMiddleware(
        [Frozen] Mock<IConnectionMultiplexer> redisMock,
        [Frozen] Mock<IDatabase> dbMock)
    {
        // Arrange
        dbMock.Setup(db => db.StringGetAsync(
                It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync("4");

        redisMock.Setup(c => c.GetDatabase(It.IsAny<int>(), null))
            .Returns(dbMock.Object);
        var httpContext = new DefaultHttpContext();
        var wasCalled = false;
        var next = new RequestDelegate((_) =>
        {
            wasCalled = true;
            return Task.CompletedTask;
        });
        var jwtOptions = new JwtOptions()
        {
            Secret = "very-strong-secret-key-for-hmac256",
            Issuer = "FinTrack",
            Audience = "FinTrackAudience",
            AccessTokenExpirationMinutes = 60,
            FirstLoginChangePasswordExpirationMinutes = 10,
            RefreshTokenExpirationDays = 7,
            InvitationTokenExpirationDays = 7
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>()
        {
            new("UserPublicId", Guid.NewGuid().ToString()),
            new("TenantPublicId", Guid.NewGuid().ToString()),
            new("JwtVersion", "3"),
            new("UserRoleInTenant", "Admin_test")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);
        httpContext.User = principal;
        var securityToken = new JwtSecurityToken(
            jwtOptions.Issuer,
            jwtOptions.Audience,
            claims,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(jwtOptions.AccessTokenExpirationMinutes),
            credentials
        );
        var token = new JwtSecurityTokenHandler().WriteToken(securityToken);
        httpContext.Request.Headers.Append("Authorization", $"Bearer {token}");
        var authOptions = new AuthenticationOptions()
        {
            AuthWhiteList = new List<string>() {     
                "/api/account/confirm-email",
                "/api/account/login",
                "/api/tenant/register" }
        };
        var authWrapper = new OptionsWrapper<AuthenticationOptions>(authOptions);
        
        var middleware = new BasicJwtTokenValidationMiddleware(next, authWrapper, redisMock.Object);
        // action
        await middleware.Invoke(httpContext);

        // assert
        wasCalled.Should().BeFalse();
        httpContext.Response.StatusCode.Should().Be(401);
    }
    
    [Theory, AutoMoqData]
    public async Task Invoke_WithValidBearerJwtToken_CallsNextMiddleware(
        [Frozen] Mock<IConnectionMultiplexer> redisMock,
        [Frozen] Mock<IDatabase> dbMock)
    {
        // Arrange
        dbMock.Setup(db => db.StringGetAsync(
                It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync("3");

        redisMock.Setup(c => c.GetDatabase(It.IsAny<int>(), null))
            .Returns(dbMock.Object);
        var httpContext = new DefaultHttpContext();
        var wasCalled = false;
        var next = new RequestDelegate((_) =>
        {
            wasCalled = true;
            return Task.CompletedTask;
        });
        var jwtOptions = new JwtOptions()
        {
            Secret = "very-strong-secret-key-for-hmac256",
            Issuer = "FinTrack",
            Audience = "FinTrackAudience",
            AccessTokenExpirationMinutes = 60,
            FirstLoginChangePasswordExpirationMinutes = 10,
            RefreshTokenExpirationDays = 7,
            InvitationTokenExpirationDays = 7
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>()
        {
            new("UserPublicId", Guid.NewGuid().ToString()),
            new("TenantPublicId", Guid.NewGuid().ToString()),
            new("JwtVersion", "3"),
            new("UserRoleInTenant", "Admin_test")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);
        httpContext.User = principal;
        var securityToken = new JwtSecurityToken(
            jwtOptions.Issuer,
            jwtOptions.Audience,
            claims,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(jwtOptions.AccessTokenExpirationMinutes),
            credentials
        );
        var token = new JwtSecurityTokenHandler().WriteToken(securityToken);
        httpContext.Request.Headers.Append("Authorization", $"Bearer {token}");
        var authOptions = new AuthenticationOptions()
        {
            AuthWhiteList = new List<string>() {     
                "/api/account/confirm-email",
                "/api/account/login",
                "/api/tenant/register" }
        };
        var authWrapper = new OptionsWrapper<AuthenticationOptions>(authOptions);
        
        var middleware = new BasicJwtTokenValidationMiddleware(next, authWrapper, redisMock.Object);
        // action
        await middleware.Invoke(httpContext);

        // assert
        wasCalled.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(200);
    }
}