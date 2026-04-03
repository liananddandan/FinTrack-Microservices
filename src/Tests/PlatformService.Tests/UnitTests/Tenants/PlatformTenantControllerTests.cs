using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PlatformService.Api.Tenants.Controllers;
using PlatformService.Application.Tenants.Dtos;
using PlatformService.Application.Tenants.Queries;
using SharedKernel.Common.Results;

namespace PlatformService.Tests.UnitTests.Tenants;

public class PlatformTenantControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact]
    public async Task GetAllAsync_Should_Send_Query_And_Return_ActionResult()
    {
        var expected = new List<TenantSummaryDto>
        {
            new()
            {
                TenantPublicId = Guid.NewGuid().ToString(),
                TenantName = "Auckland Coffee",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mediator
            .Setup(x => x.Send(
                It.IsAny<GetAllPlatformTenantsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<IReadOnlyList<TenantSummaryDto>>.Ok(
                expected,
                "Platform.Tenant.GetAllSuccess",
                "Tenants retrieved successfully."));

        var controller = new PlatformTenantController(_mediator.Object);

        var result = await controller.GetAllAsync(CancellationToken.None);

        result.Should().BeAssignableTo<IActionResult>();

        _mediator.Verify(x => x.Send(
            It.IsAny<GetAllPlatformTenantsQuery>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}