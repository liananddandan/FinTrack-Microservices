using AutoFixture.Xunit2;
using FluentAssertions;
using IdentityService.Application.Common.Abstractions;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Common.Filters;
using IdentityService.Application.Common.Filters.Attributes;
using IdentityService.Application.Common.Status;
using IdentityService.Application.Tenants.Abstractions;
using IdentityService.Domain.Entities;
using IdentityService.Tests.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using SharedKernel.Common.Constants;
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
            .ReturnsAsync(ServiceResult<JwtParseDto>.Fail("empty token", "empty token"));
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
            .ReturnsAsync(ServiceResult<JwtParseDto>.Fail("invalidToken", "Invalid token"));
        
        // Act
        await sut.OnAuthorizationAsync(context);

        // Assert
        Assert.IsType<JsonResult>(context.Result);
        jwtTokenServiceMock.Verify(jts => jts.ParseJwtTokenAsync("invalidToken"), Times.Once);
    }
    
    [Theory, AutoMoqData]
    public async Task OnAuthorizationAsync_ShouldReturnUnAuthorizationResult_WhenInviteIsEmpty(
        [Frozen] Mock<IJwtTokenService> jwtTokenServiceMock,
        GlobalJwtTokenValidationFilter sut)
    {
        jwtTokenServiceMock.Setup(jts => jts.ParseInvitationTokenAsync(""))
            .ReturnsAsync(ServiceResult<InvitationParseDto>.Fail("empty token", "empty token"));
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
            .ReturnsAsync(ServiceResult<InvitationParseDto>.Fail("invalidToken", "Invalid token"));
        
        // Act
        await sut.OnAuthorizationAsync(context);

        // Assert
        Assert.IsType<JsonResult>(context.Result);
        jwtTokenServiceMock.Verify(jts => jts.ParseInvitationTokenAsync("invalidToken"), Times.Once);
    }
    
    [Theory, AutoMoqData]
    public async Task OnAuthorizationAsync_ShouldReturnForbidResult_WhenInvitationCouldNotFound(
        [Frozen] Mock<IJwtTokenService> jwtTokenServiceMock,
        [Frozen] Mock<ITenantInvitationService> tenantInvitationServiceMock,
        InvitationParseDto invitationParseDto,
        GlobalJwtTokenValidationFilter sut)
    {
        // Arrange
        invitationParseDto.TokenType = JwtTokenType.InvitationToken;
        var context = CreateAuthorizationFilterContext(new RequireTokenTypeAttribute(JwtTokenType.InvitationToken));
        context.HttpContext.Request.Headers.Authorization = "Invite GoodToken";
        jwtTokenServiceMock.Setup(jts => jts.ParseInvitationTokenAsync("GoodToken"))
            .ReturnsAsync(ServiceResult<InvitationParseDto>.Ok(invitationParseDto,"good token", "good token"));
        tenantInvitationServiceMock
            .Setup(tis => tis.GetTenantInvitationByPublicIdAsync(invitationParseDto.InvitationPublicId, CancellationToken.None))
            .ReturnsAsync(ServiceResult<TenantInvitation>.Fail("fail", "fail"));
        // Act
        await sut.OnAuthorizationAsync(context);

        // Assert
        Assert.IsType<UnauthorizedResult>(context.Result);
    }
    
    [Theory, AutoMoqData]
    public async Task OnAuthorizationAsync_ShouldReturnSuccess_WhenInvitationTokenValid(
        [Frozen] Mock<IJwtTokenService> jwtTokenServiceMock,
        [Frozen] Mock<ITenantInvitationService> tenantInvitationServiceMock,
        InvitationParseDto invitationParseDto,
        GlobalJwtTokenValidationFilter sut)
    {
        // Arrange
        invitationParseDto.InvitationVersion = "5";
        invitationParseDto.TokenType = JwtTokenType.InvitationToken;

        var tenantInvitation = new TenantInvitation
        {
            PublicId = Guid.TryParse(invitationParseDto.InvitationPublicId, out var parsedId)
                ? parsedId
                : Guid.NewGuid(),
            Email = "invitee@test.com",
            Version = 5,
            ExpiredAt = DateTime.UtcNow.AddDays(7),
            Status = InvitationStatus.Pending
        };

        var context = CreateAuthorizationFilterContext(
            new RequireTokenTypeAttribute(JwtTokenType.InvitationToken));

        context.HttpContext.Request.Headers.Authorization = "Invite GoodToken";

        jwtTokenServiceMock
            .Setup(x => x.ParseInvitationTokenAsync("GoodToken"))
            .ReturnsAsync(ServiceResult<InvitationParseDto>.Ok(
                invitationParseDto,
                "good token",
                "good token"));

        tenantInvitationServiceMock
            .Setup(x => x.GetTenantInvitationByPublicIdAsync(
                invitationParseDto.InvitationPublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TenantInvitation>.Ok(
                tenantInvitation,
                "good",
                "good"));

        // Act
        await sut.OnAuthorizationAsync(context);

        // Assert
        Assert.IsNotType<UnauthorizedResult>(context.Result);
        Assert.IsNotType<ForbidResult>(context.Result);
        context.HttpContext.Items["InviteParseResult"].Should().NotBeNull();
        context.HttpContext.Items["InviteParseResult"].Should().BeEquivalentTo(invitationParseDto);
    }
    
}