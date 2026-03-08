using System.Security.Claims;
using AuditLogService.Api.Controllers;
using AuditLogService.Application.DTOs;
using AuditLogService.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AuditLogService.Tests.Api.IntegrationTests;

public class AuditLogsGetTests
{
    [Fact]
    public async Task QueryAsync_Should_Return_Ok_With_Result()
    {
        var readerMock = new Mock<IAuditLogReader>();

        readerMock
            .Setup(x => x.QueryAsync(
                It.IsAny<AuditLogQueryRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<AuditLogDto>
            {
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 20,
                Items =
                [
                    new AuditLogDto
                    {
                        PublicId = Guid.NewGuid().ToString(),
                        TenantPublicId = "tenant-001",
                        ActionType = "Membership.Invited",
                        Category = "Membership",
                        Summary = "Emily invited foo@test.com."
                    }
                ]
            });

        var sut = new AuditLogsController(readerMock.Object);

        var claims = new List<Claim>
        {
            new("tenant", "tenant-001"),
            new(ClaimTypes.Role, "Admin")
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };

        var result = await sut.QueryAsync(
            actionType: "Membership.Invited",
            actorUserPublicId: null,
            targetPublicId: null,
            fromUtc: null,
            toUtc: null,
            pageNumber: 1,
            pageSize: 20,
            cancellationToken: CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = okResult.Value.Should().BeOfType<PagedResult<AuditLogDto>>().Subject;

        payload.TotalCount.Should().Be(1);
        payload.Items.Should().HaveCount(1);

        readerMock.Verify(x => x.QueryAsync(
                It.Is<AuditLogQueryRequest>(q => q.TenantPublicId == "tenant-001"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task QueryAsync_Should_Return_Unauthorized_When_Tenant_Claim_Is_Missing()
    {
        var readerMock = new Mock<IAuditLogReader>();
        var sut = new AuditLogsController(readerMock.Object);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, "Admin")
        };

        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"))
            }
        };

        var result = await sut.QueryAsync(
            null, null, null, null, null, 1, 20, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }
    
    [Fact]
    public async Task QueryAsync_Should_Return_Forbid_When_User_Is_Not_Admin()
    {
        var readerMock = new Mock<IAuditLogReader>();
        var sut = new AuditLogsController(readerMock.Object);

        var claims = new List<Claim>
        {
            new("tenant", "tenant-001"),
            new(ClaimTypes.Role, "Member")
        };

        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"))
            }
        };

        var result = await sut.QueryAsync(
            null, null, null, null, null, 1, 20, CancellationToken.None);

        result.Should().BeOfType<ForbidResult>();
    }
    
    [Fact]
    public void Ping_Should_Return_Ok()
    {
        var readerMock = new Mock<IAuditLogReader>();
        var sut = new AuditLogsController(readerMock.Object);

        var result = sut.Ping();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be("AuditLogService is running.");
    }
}