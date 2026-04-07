using FluentAssertions;
using IdentityService.Api.Accounts.Controllers;
using IdentityService.Application.Accounts.Commands;
using IdentityService.Application.Accounts.Dtos;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Platforms.Commands;
using IdentityService.Application.Platforms.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SharedKernel.Common.Constants;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;

namespace IdentityService.Tests.UnitTests.Account;

public class AccountControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    private AccountController CreateControllerWithHttpContext(DefaultHttpContext httpContext)
    {
        var controller = new AccountController(_mediator.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };

        return controller;
    }

    [Fact]
    public async Task SelectPlatformAsync_Should_Return_Unauthorized_When_JwtParseResult_Missing()
    {
        var httpContext = new DefaultHttpContext();
        var controller = CreateControllerWithHttpContext(httpContext);

        var result = await controller.SelectPlatformAsync();

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task SelectPlatformAsync_Should_Send_Command_And_Return_Result_When_JwtParseResult_Exists()
    {
        var userPublicId = Guid.NewGuid().ToString();

        var httpContext = new DefaultHttpContext();
        httpContext.Items["JwtParseResult"] = new JwtParseDto
        {
            UserPublicId = userPublicId,
            JwtVersion = "1",
            TenantPublicId = string.Empty,
            UserRoleInTenant = string.Empty,
            TokenType = JwtTokenType.AccountAccessToken,
            HasPlatformAccess = false,
            PlatformRole = string.Empty
        };

        var expectedDto = new PlatformTokenDto
        {
            PlatformAccessToken = "platform-token",
            PlatformRole = "SuperAdmin"
        };

        _mediator
            .Setup(x => x.Send(
                It.Is<SelectPlatformCommand>(c => c.UserPublicId == userPublicId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<PlatformTokenDto>.Ok(
                expectedDto,
                "Identity.PlatformAccess.SelectSuccess",
                "Platform access token generated successfully."));

        var controller = CreateControllerWithHttpContext(httpContext);

        var result = await controller.SelectPlatformAsync();

        Assert.IsAssignableFrom<IActionResult>(result);

        _mediator.Verify(x => x.Send(
            It.Is<SelectPlatformCommand>(c => c.UserPublicId == userPublicId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task VerifyEmail_Should_Return_BadRequest_When_Verification_Fails()
    {
        var serviceResult = ServiceResult<bool>.Fail(
            "EMAIL_VERIFICATION_TOKEN_INVALID",
            "Verification token is invalid.");

        _mediator
            .Setup(x => x.Send(
                It.IsAny<VerifyEmailCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var request = new VerifyEmailRequest("invalid-token");
        var controller = CreateControllerWithHttpContext(new DefaultHttpContext());

        var result = await controller.VerifyEmail(request, CancellationToken.None);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var value = badRequestResult.Value.Should().BeAssignableTo<ApiResponse<bool>>().Subject;

        value.Data.Should().BeFalse();
        value.Code.Should().Be("EMAIL_VERIFICATION_TOKEN_INVALID");
        value.Message.Should().Be("Verification token is invalid.");
        value.Data.Should().BeFalse();
    }
    
}