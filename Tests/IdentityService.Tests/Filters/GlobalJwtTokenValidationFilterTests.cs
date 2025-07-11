using AutoFixture.Xunit2;
using FluentAssertions;
using IdentityService.Common.DTOs;
using IdentityService.Common.Status;
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
    public async Task OnAuthorizationAsync_ShouldReturnUnAuthorizationResult_WhenNoBearer(
        GlobalJwtTokenValidationFilter sut)
    {
        var context = CreateAuthorizationFilterContext();
        
        await sut.OnAuthorizationAsync(context);

        Assert.IsType<UnauthorizedResult>(context.Result);
    }
    
    [Theory, AutoMoqData]
    public async Task OnAuthorizationAsync_ShouldReturnUnAuthorizationResult_WhenBearerIsEmpty(
        GlobalJwtTokenValidationFilter sut)
    {
        var context = CreateAuthorizationFilterContext();
        context.HttpContext.Request.Headers.Authorization = "Bearer ";
        
        await sut.OnAuthorizationAsync(context);

        Assert.IsType<UnauthorizedResult>(context.Result);
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
        Assert.IsType<UnauthorizedResult>(context.Result);
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
    public async Task OnAuthorizationAsync_ShouldReturnSuccess_WhenTokenValid(
        [Frozen] Mock<IJwtTokenService> jwtTokenServiceMock,
        JwtParseResult jwtParseResult,
        GlobalJwtTokenValidationFilter sut)
    {
        // Arrange
        jwtParseResult.TokenType = JwtTokenType.AccessToken;
        var context = CreateAuthorizationFilterContext(new RequireTokenTypeAttribute(JwtTokenType.AccessToken));
        context.HttpContext.Request.Headers.Authorization = "Bearer fine-token-in-request";
        jwtTokenServiceMock.Setup(jts => jts.ParseJwtTokenAsync("fine-token-in-request"))
            .ReturnsAsync(ServiceResult<JwtParseResult>.Ok(jwtParseResult,"good token", "good token"));
        
        // Act
        await sut.OnAuthorizationAsync(context);

        // Assert
        Assert.IsNotType<UnauthorizedResult>(context.Result);
        Assert.IsNotType<ForbidResult>(context.Result);
        context.HttpContext.Items["JwtParseResult"].Should().NotBeNull();
        context.HttpContext.Items["JwtParseResult"].Should().BeEquivalentTo(jwtParseResult);
    }
}