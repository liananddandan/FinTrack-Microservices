using AutoFixture.Xunit2;
using FluentAssertions;
using IdentityService.Common.DTOs;
using IdentityService.Common.Status;
using IdentityService.Domain.Entities;
using IdentityService.Filters;
using IdentityService.Filters.Attributes;
using IdentityService.Services.Interfaces;
using IdentityService.Tests.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using SharedKernel.Common.Results;

namespace IdentityService.Tests.Filters;

public class GlobalJwtTokenValidationFilterTests
{
    private static AuthorizationFilterContext CreateAuthorizationFilterContext(params object[] attributes)
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext
        {
            HttpContext = httpContext,
            RouteData = new RouteData(),
            ActionDescriptor = new ControllerActionDescriptor()
            {
                EndpointMetadata = attributes.ToList()
            }
        };
        return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
    }

    [Theory, AutoMoqData]
    public async Task OnAuthorizationAsync_ShouldNotReturnUnAuthorizationResult_WhenHasAllowAnonymousToken(
        GlobalJwtTokenValidationFilter sut)
    {
        var context = CreateAuthorizationFilterContext(new AllowAnonymousTokenAttribute());
        
        await sut.OnAuthorizationAsync(context);

        Assert.IsNotType<UnauthorizedResult>(context.Result);
    }
    
    [Theory, AutoMoqData]
    public async Task OnAuthorizationAsync_ShouldReturnUnAuthorizationResult_WhenNoAuthorization(
        GlobalJwtTokenValidationFilter sut)
    {
        var context = CreateAuthorizationFilterContext();
        
        await sut.OnAuthorizationAsync(context);

        Assert.IsType<UnauthorizedResult>(context.Result);
    }
    
    [Theory, AutoMoqData]
    public async Task OnAuthorizationAsync_ShouldReturnUnAuthorizationResult_WhenAuthorizationWithoutValidPrefix(
        GlobalJwtTokenValidationFilter sut)
    {
        var context = CreateAuthorizationFilterContext();
        context.HttpContext.Request.Headers.Authorization = "Invalid ";
        
        await sut.OnAuthorizationAsync(context);

        Assert.IsType<UnauthorizedResult>(context.Result);
    }
    
    [Theory, AutoMoqData]
    public async Task OnAuthorizationAsync_ShouldReturnUnAuthorizationResult_WhenBearerIsEmpty(
        [Frozen] Mock<IJwtTokenService> jwtTokenServiceMock,
        GlobalJwtTokenValidationFilter sut)
    {
        // Arrange
        jwtTokenServiceMock.Setup(jts => jts.ParseJwtTokenAsync(""))
            .ReturnsAsync(ServiceResult<JwtParseResult>.Fail("empty token", "empty token"));
        var context = CreateAuthorizationFilterContext();
        context.HttpContext.Request.Headers.Authorization = "Bearer ";
        
        await sut.OnAuthorizationAsync(context);

        Assert.IsType<JsonResult>(context.Result);
    }
    
    [Theory, AutoMoqData]
    public async Task OnAuthorizationAsync_ShouldReturnUnAuthorizationResult_WhenTokenIsInvalid(
        [Frozen] Mock<IJwtTokenService> jwtTokenServiceMock,
        GlobalJwtTokenValidationFilter sut)
    {
        // Arrange
        var context = CreateAuthorizationFilterContext();
        context.HttpContext.Request.Headers.Authorization= "Bearer invalidToken";
        jwtTokenServiceMock.Setup(jts => jts.ParseJwtTokenAsync("invalidToken"))
            .ReturnsAsync(ServiceResult<JwtParseResult>.Fail("invalidToken", "Invalid token"));
        
        // Act
        await sut.OnAuthorizationAsync(context);

        // Assert
        Assert.IsType<JsonResult>(context.Result);
        jwtTokenServiceMock.Verify(jts => jts.ParseJwtTokenAsync("invalidToken"), Times.Once);
    }
    
    [Theory, AutoMoqData]
    public async Task OnAuthorizationAsync_ShouldReturnUnAuthorization_WhenTokenParsedResultIsNull(
        [Frozen] Mock<IJwtTokenService> jwtTokenServiceMock,
        JwtParseResult jwtParseResult,
        GlobalJwtTokenValidationFilter sut)
    {
        // Arrange
        jwtParseResult.TokenType = JwtTokenType.AccessToken;
        var context = CreateAuthorizationFilterContext(new RequireTokenTypeAttribute(JwtTokenType.FirstLoginToken));
        context.HttpContext.Request.Headers.Authorization = "Bearer WrongTypeToken";
        jwtTokenServiceMock.Setup(jts => jts.ParseJwtTokenAsync("WrongTypeToken"))
            .ReturnsAsync(ServiceResult<JwtParseResult>.Ok(null,"wrong token type", "wrong token type"));
        
        // Act
        await sut.OnAuthorizationAsync(context);

        // Assert
        Assert.IsType<JsonResult>(context.Result);
    }
    
    [Theory, AutoMoqData]
    public async Task OnAuthorizationAsync_ShouldReturnForbidResult_WhenTokenTypeIsInvalid(
        [Frozen] Mock<IJwtTokenService> jwtTokenServiceMock,
        JwtParseResult jwtParseResult,
        GlobalJwtTokenValidationFilter sut)
    {
        // Arrange
        jwtParseResult.TokenType = JwtTokenType.AccessToken;
        var context = CreateAuthorizationFilterContext(new RequireTokenTypeAttribute(JwtTokenType.FirstLoginToken));
        context.HttpContext.Request.Headers.Authorization = "Bearer WrongTypeToken";
        jwtTokenServiceMock.Setup(jts => jts.ParseJwtTokenAsync("WrongTypeToken"))
            .ReturnsAsync(ServiceResult<JwtParseResult>.Ok(jwtParseResult,"wrong token type", "wrong token type"));
        
        // Act
        await sut.OnAuthorizationAsync(context);

        // Assert
        Assert.IsType<ForbidResult>(context.Result);
    }
    
    [Theory, AutoMoqData]
    public async Task OnAuthorizationAsync_ShouldReturnUnauthorizedResult_WhenUserNotFound(
        [Frozen] Mock<IJwtTokenService> jwtTokenServiceMock,
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        JwtParseResult jwtParseResult,
        GlobalJwtTokenValidationFilter sut)
    {
        // Arrange
        jwtParseResult.TokenType = JwtTokenType.AccessToken;
        var context = CreateAuthorizationFilterContext(new RequireTokenTypeAttribute(JwtTokenType.AccessToken));
        context.HttpContext.Request.Headers.Authorization = "Bearer GoodToken";
        jwtTokenServiceMock.Setup(jts => jts.ParseJwtTokenAsync("GoodToken"))
            .ReturnsAsync(ServiceResult<JwtParseResult>
                .Ok(jwtParseResult,"good token", "good type"));
        userDomainServiceMock
            .Setup(uds =>
                uds.GetUserByPublicIdIncludeTenantAsync(jwtParseResult.UserPublicId, CancellationToken.None))
            .ReturnsAsync((ApplicationUser?)null);
        // Act
        await sut.OnAuthorizationAsync(context);

        // Assert
        Assert.IsType<UnauthorizedResult>(context.Result);
    }
    
    [Theory, AutoMoqData]
    public async Task OnAuthorizationAsync_ShouldReturnUnauthorizedResult_WhenJwtTokenCanNotParsed(
        [Frozen] Mock<IJwtTokenService> jwtTokenServiceMock,
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        JwtParseResult jwtParseResult,
        ApplicationUser user,
        GlobalJwtTokenValidationFilter sut)
    {
        // Arrange
        jwtParseResult.TokenType = JwtTokenType.AccessToken;
        jwtParseResult.JwtVersion = "3s";
        user.JwtVersion = 4;
        var context = CreateAuthorizationFilterContext(new RequireTokenTypeAttribute(JwtTokenType.AccessToken));
        context.HttpContext.Request.Headers.Authorization = "Bearer GoodToken";
        jwtTokenServiceMock.Setup(jts => jts.ParseJwtTokenAsync("GoodToken"))
            .ReturnsAsync(ServiceResult<JwtParseResult>
                .Ok(jwtParseResult,"good token", "good type"));
        userDomainServiceMock
            .Setup(uds =>
                uds.GetUserByPublicIdIncludeTenantAsync(jwtParseResult.UserPublicId, CancellationToken.None))
            .ReturnsAsync(user);
        // Act
        await sut.OnAuthorizationAsync(context);

        // Assert
        Assert.IsType<UnauthorizedResult>(context.Result);
    }
    
    [Theory, AutoMoqData]
    public async Task OnAuthorizationAsync_ShouldReturnUnauthorizedResult_WhenJwtTokenIsNotValid(
        [Frozen] Mock<IJwtTokenService> jwtTokenServiceMock,
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        JwtParseResult jwtParseResult,
        ApplicationUser user,
        GlobalJwtTokenValidationFilter sut)
    {
        // Arrange
        jwtParseResult.TokenType = JwtTokenType.AccessToken;
        jwtParseResult.JwtVersion = "3";
        user.JwtVersion = 4;
        var context = CreateAuthorizationFilterContext(new RequireTokenTypeAttribute(JwtTokenType.AccessToken));
        context.HttpContext.Request.Headers.Authorization = "Bearer GoodToken";
        jwtTokenServiceMock.Setup(jts => jts.ParseJwtTokenAsync("GoodToken"))
            .ReturnsAsync(ServiceResult<JwtParseResult>
                .Ok(jwtParseResult,"good token", "good type"));
        userDomainServiceMock
            .Setup(uds =>
                uds.GetUserByPublicIdIncludeTenantAsync(jwtParseResult.UserPublicId, CancellationToken.None))
            .ReturnsAsync(user);
        // Act
        await sut.OnAuthorizationAsync(context);

        // Assert
        Assert.IsType<UnauthorizedResult>(context.Result);
    }
    
    [Theory, AutoMoqData]
    public async Task OnAuthorizationAsync_ShouldReturnSuccess_WhenTokenValid(
        [Frozen] Mock<IJwtTokenService> jwtTokenServiceMock,
        [Frozen] Mock<IUserDomainService> userDomainServiceMock,
        JwtParseResult jwtParseResult,
        ApplicationUser user,
        GlobalJwtTokenValidationFilter sut)
    {
        // Arrange
        jwtParseResult.TokenType = JwtTokenType.AccessToken;
        jwtParseResult.JwtVersion = "3";
        user.JwtVersion = 3;
        var context = CreateAuthorizationFilterContext(new RequireTokenTypeAttribute(JwtTokenType.AccessToken));
        context.HttpContext.Request.Headers.Authorization = "Bearer fine-token-in-request";
        jwtTokenServiceMock.Setup(jts => jts.ParseJwtTokenAsync("fine-token-in-request"))
            .ReturnsAsync(ServiceResult<JwtParseResult>.Ok(jwtParseResult,"good token", "good token"));
        userDomainServiceMock
            .Setup(uds =>
                uds.GetUserByPublicIdIncludeTenantAsync(jwtParseResult.UserPublicId, CancellationToken.None))
            .ReturnsAsync(user);
        // Act
        await sut.OnAuthorizationAsync(context);

        // Assert
        Assert.IsNotType<UnauthorizedResult>(context.Result);
        Assert.IsNotType<ForbidResult>(context.Result);
        context.HttpContext.Items["JwtParseResult"].Should().NotBeNull();
        context.HttpContext.Items["JwtParseResult"].Should().BeEquivalentTo(jwtParseResult);
    }
    
    [Theory, AutoMoqData]
    public async Task OnAuthorizationAsync_ShouldReturnUnAuthorizationResult_WhenInviteIsEmpty(
        [Frozen] Mock<IJwtTokenService> jwtTokenServiceMock,
        GlobalJwtTokenValidationFilter sut)
    {
        jwtTokenServiceMock.Setup(jts => jts.ParseInvitationTokenAsync(""))
            .ReturnsAsync(ServiceResult<InvitationParseResult>.Fail("empty token", "empty token"));
        var context = CreateAuthorizationFilterContext();
        context.HttpContext.Request.Headers.Authorization = "Invite ";
        
        await sut.OnAuthorizationAsync(context);

        Assert.IsType<JsonResult>(context.Result);
    }
    
    [Theory, AutoMoqData]
    public async Task OnAuthorizationAsync_ShouldReturnUnAuthorizationResult_WhenInviteTokenIsInvalid(
        [Frozen] Mock<IJwtTokenService> jwtTokenServiceMock,
        GlobalJwtTokenValidationFilter sut)
    {
        // Arrange
        var context = CreateAuthorizationFilterContext();
        context.HttpContext.Request.Headers.Authorization= "Invite invalidToken";
        jwtTokenServiceMock.Setup(jts => jts.ParseInvitationTokenAsync("invalidToken"))
            .ReturnsAsync(ServiceResult<InvitationParseResult>.Fail("invalidToken", "Invalid token"));
        
        // Act
        await sut.OnAuthorizationAsync(context);

        // Assert
        Assert.IsType<JsonResult>(context.Result);
        jwtTokenServiceMock.Verify(jts => jts.ParseInvitationTokenAsync("invalidToken"), Times.Once);
    }
    
    [Theory, AutoMoqData]
    public async Task OnAuthorizationAsync_ShouldReturnUnAuthorization_WhenInvitationParsedResultIsNull(
        [Frozen] Mock<IJwtTokenService> jwtTokenServiceMock,
        JwtParseResult jwtParseResult,
        GlobalJwtTokenValidationFilter sut)
    {
        // Arrange
        jwtParseResult.TokenType = JwtTokenType.AccessToken;
        var context = CreateAuthorizationFilterContext(new RequireTokenTypeAttribute(JwtTokenType.FirstLoginToken));
        context.HttpContext.Request.Headers.Authorization = "Invite WrongTypeToken";
        jwtTokenServiceMock.Setup(jts => jts.ParseInvitationTokenAsync("WrongTypeToken"))
            .ReturnsAsync(ServiceResult<InvitationParseResult>.Ok(null,"wrong token type", "wrong token type"));
        
        // Act
        await sut.OnAuthorizationAsync(context);

        // Assert
        Assert.IsType<JsonResult>(context.Result);
    }
    
    [Theory, AutoMoqData]
    public async Task OnAuthorizationAsync_ShouldReturnForbidResult_WhenInvitationTypeIsInvalid(
        [Frozen] Mock<IJwtTokenService> jwtTokenServiceMock,
        InvitationParseResult invitationParseResult,
        GlobalJwtTokenValidationFilter sut)
    {
        // Arrange
        invitationParseResult.TokenType = JwtTokenType.InvitationToken;
        var context = CreateAuthorizationFilterContext(new RequireTokenTypeAttribute(JwtTokenType.FirstLoginToken));
        context.HttpContext.Request.Headers.Authorization = "Bearer WrongTypeToken";
        jwtTokenServiceMock.Setup(jts => jts.ParseInvitationTokenAsync("WrongTypeToken"))
            .ReturnsAsync(ServiceResult<InvitationParseResult>.Ok(invitationParseResult,"wrong token type", "wrong token type"));
        
        // Act
        await sut.OnAuthorizationAsync(context);

        // Assert
        Assert.IsType<ForbidResult>(context.Result);
    }
    
    [Theory, AutoMoqData]
    public async Task OnAuthorizationAsync_ShouldReturnForbidResult_WhenInvitationCouldNotFound(
        [Frozen] Mock<IJwtTokenService> jwtTokenServiceMock,
        [Frozen] Mock<ITenantInvitationService> tenantInvitationServiceMock,
        InvitationParseResult invitationParseResult,
        GlobalJwtTokenValidationFilter sut)
    {
        // Arrange
        invitationParseResult.TokenType = JwtTokenType.InvitationToken;
        var context = CreateAuthorizationFilterContext(new RequireTokenTypeAttribute(JwtTokenType.InvitationToken));
        context.HttpContext.Request.Headers.Authorization = "Invite GoodToken";
        jwtTokenServiceMock.Setup(jts => jts.ParseInvitationTokenAsync("GoodToken"))
            .ReturnsAsync(ServiceResult<InvitationParseResult>.Ok(invitationParseResult,"good token", "good token"));
        tenantInvitationServiceMock
            .Setup(tis => tis.GetTenantInvitationByPublicIdAsync(invitationParseResult.InvitationPublicId, CancellationToken.None))
            .ReturnsAsync(ServiceResult<TenantInvitation>.Fail("fail", "fail"));
        // Act
        await sut.OnAuthorizationAsync(context);

        // Assert
        Assert.IsType<UnauthorizedResult>(context.Result);
    }
    
    [Theory, AutoMoqData]
    public async Task OnAuthorizationAsync_ShouldReturnForbidResult_WhenInvitationVersionInvalid(
        [Frozen] Mock<IJwtTokenService> jwtTokenServiceMock,
        [Frozen] Mock<ITenantInvitationService> tenantInvitationServiceMock,
        InvitationParseResult invitationParseResult,
        TenantInvitation tenantInvitation,
        GlobalJwtTokenValidationFilter sut)
    {
        // Arrange
        tenantInvitation.Version = 5;
        invitationParseResult.InvitationVersion = "4";
        invitationParseResult.TokenType = JwtTokenType.InvitationToken;
        var context = CreateAuthorizationFilterContext(new RequireTokenTypeAttribute(JwtTokenType.InvitationToken));
        context.HttpContext.Request.Headers.Authorization = "Invite GoodToken";
        jwtTokenServiceMock.Setup(jts => jts.ParseInvitationTokenAsync("GoodToken"))
            .ReturnsAsync(ServiceResult<InvitationParseResult>.Ok(invitationParseResult,"good token", "good token"));
        tenantInvitationServiceMock
            .Setup(tis => tis.GetTenantInvitationByPublicIdAsync(invitationParseResult.InvitationPublicId, CancellationToken.None))
            .ReturnsAsync(ServiceResult<TenantInvitation>.Ok(tenantInvitation,"good", "good"));
        // Act
        await sut.OnAuthorizationAsync(context);

        // Assert
        Assert.IsType<UnauthorizedResult>(context.Result);
    }
    
    [Theory, AutoMoqData]
    public async Task OnAuthorizationAsync_ShouldReturnSuccess_WhenInvitationTokenValid(
        [Frozen] Mock<IJwtTokenService> jwtTokenServiceMock,
        [Frozen] Mock<ITenantInvitationService> tenantInvitationServiceMock,
        InvitationParseResult invitationParseResult,
        TenantInvitation tenantInvitation,
        GlobalJwtTokenValidationFilter sut)
    {
        // Arrange
        tenantInvitation.Version = 5;
        invitationParseResult.InvitationVersion = "5";
        invitationParseResult.TokenType = JwtTokenType.InvitationToken;
        var context = CreateAuthorizationFilterContext(new RequireTokenTypeAttribute(JwtTokenType.InvitationToken));
        context.HttpContext.Request.Headers.Authorization = "Invite GoodToken";
        jwtTokenServiceMock.Setup(jts => jts.ParseInvitationTokenAsync("GoodToken"))
            .ReturnsAsync(ServiceResult<InvitationParseResult>.Ok(invitationParseResult,"good token", "good token"));
        tenantInvitationServiceMock
            .Setup(tis => tis.GetTenantInvitationByPublicIdAsync(invitationParseResult.InvitationPublicId, CancellationToken.None))
            .ReturnsAsync(ServiceResult<TenantInvitation>.Ok(tenantInvitation,"good", "good"));
        // Act
        await sut.OnAuthorizationAsync(context);

        // Assert
        Assert.IsNotType<UnauthorizedResult>(context.Result);
        Assert.IsNotType<ForbidResult>(context.Result);
        context.HttpContext.Items["InviteParseResult"].Should().NotBeNull();
        context.HttpContext.Items["InviteParseResult"].Should().BeEquivalentTo(invitationParseResult);
    }
}