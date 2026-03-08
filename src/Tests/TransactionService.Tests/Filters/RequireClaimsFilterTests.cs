using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using TransactionService.Filters;
using TransactionService.Tests.Attributes;

namespace TransactionService.Tests.Filters;

public class RequireClaimsFilterTests
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
    public async Task OnAuthorization_ShouldNotReturnNotUnAuthorization_WhenHasAllowAnonymousToken(
        RequireClaimsFilter sut)
    {
        // arrange
        var context = CreateAuthorizationFilterContext(new AllowAnonymousAttribute());
        
        // act
        await sut.OnAuthorizationAsync(context);
        
        // assert
        Assert.IsNotType<UnauthorizedResult>(context.Result);
    }
    
    [Theory, AutoMoqData]
    public async Task OnAuthorization_ShouldReturnUnAuthorization_WhenAllClaimMissed(
        RequireClaimsFilter sut)
    {
        // arrange
        var context = CreateAuthorizationFilterContext();
        
        // act
        await sut.OnAuthorizationAsync(context);
        
        // assert
        Assert.IsType<UnauthorizedResult>(context.Result);
    }
    
    [Theory, AutoMoqData]
    public async Task OnAuthorization_ShouldReturnUnAuthorization_WhenSomeClaimMissed(
        RequireClaimsFilter sut)
    {
        // arrange
        var context = CreateAuthorizationFilterContext();
        var claims = new List<Claim>
        {
            new Claim("TenantPublicId", "tenant-123"),
            new Claim("JwtVersion", "v1"),
            new Claim("UserRoleInTenant", "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var user = new ClaimsPrincipal(identity);
        context.HttpContext.User = user;
        
        // act
        await sut.OnAuthorizationAsync(context);
        
        // assert
        Assert.IsType<UnauthorizedResult>(context.Result);
    }
    
    [Theory, AutoMoqData]
    public async Task OnAuthorization_ShouldNotReturnUnAuthorization_WhenNoClaimMissed(
        RequireClaimsFilter sut)
    {
        // arrange
        var context = CreateAuthorizationFilterContext();
        var claims = new List<Claim>
        {
            new Claim("TenantPublicId", "tenant-123"),
            new Claim("UserPublicId", "user-123"),
            new Claim("JwtVersion", "v1"),
            new Claim("UserRoleInTenant", "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var user = new ClaimsPrincipal(identity);
        context.HttpContext.User = user;
        
        // act
        await sut.OnAuthorizationAsync(context);
        
        // assert
        Assert.IsNotType<UnauthorizedResult>(context.Result);
    }
}