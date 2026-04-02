using FluentAssertions;
using IdentityService.Api.Tenants.Controllers;
using IdentityService.Application.Tenants.Dtos;
using IdentityService.Application.Tenants.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SharedKernel.Common.Results;

namespace IdentityService.Tests.UnitTests.Tenant;

public class InternalTenantControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact]
    public async Task GetAllAsync_Should_Return_ActionResult_From_Mediator()
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
                It.IsAny<GetAllTenantsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<IReadOnlyList<TenantSummaryDto>>.Ok(
                expected,
                "Identity.Tenant.GetAllSuccess",
                "Tenants retrieved successfully."));

        var controller = new InternalTenantController(_mediator.Object);

        var result = await controller.GetAllAsync(CancellationToken.None);

        result.Should().BeAssignableTo<IActionResult>();

        _mediator.Verify(x => x.Send(
            It.IsAny<GetAllTenantsQuery>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}