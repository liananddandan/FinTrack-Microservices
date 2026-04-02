using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PlatformService.Api.Tenants.Contracts;
using PlatformService.Api.Tenants.Controllers;
using PlatformService.Application.Tenants.Commands;
using PlatformService.Application.Tenants.Dtos;
using PlatformService.Application.Tenants.Queries;
using SharedKernel.Common.Results;

namespace PlatformService.Tests.UnitTests.Tenants;



public class TenantDomainMappingControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact]
    public async Task GetByTenantAsync_Should_Send_Query()
    {
        var tenantPublicId = Guid.NewGuid();

        _mediator
            .Setup(x => x.Send(
                It.IsAny<GetTenantDomainMappingsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<IReadOnlyList<TenantDomainMappingDto>>.Ok(
                new List<TenantDomainMappingDto>(),
                "Platform.TenantDomain.GetSuccess",
                "ok"));

        var controller = new TenantDomainMappingController(_mediator.Object);

        var result = await controller.GetByTenantAsync(tenantPublicId, CancellationToken.None);

        result.Should().BeAssignableTo<IActionResult>();

        _mediator.Verify(x => x.Send(
            It.Is<GetTenantDomainMappingsQuery>(q => q.TenantPublicId == tenantPublicId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Should_Send_Command()
    {
        var tenantPublicId = Guid.NewGuid();

        var request = new CreateTenantDomainMappingRequest
        {
            TenantPublicId = tenantPublicId,
            Host = "coffee.chenlis.com",
            DomainType = "TenantPortal",
            IsPrimary = true,
            IsActive = true
        };

        _mediator
            .Setup(x => x.Send(
                It.IsAny<CreateTenantDomainMappingCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TenantDomainMappingDto>.Ok(
                new TenantDomainMappingDto
                {
                    DomainPublicId = Guid.NewGuid(),
                    TenantPublicId = tenantPublicId,
                    Host = "coffee.chenlis.com",
                    DomainType = "TenantPortal",
                    IsPrimary = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                "Platform.TenantDomain.CreateSuccess",
                "ok"));

        var controller = new TenantDomainMappingController(_mediator.Object);

        var result = await controller.CreateAsync(request, CancellationToken.None);

        result.Should().BeAssignableTo<IActionResult>();

        _mediator.Verify(x => x.Send(
            It.Is<CreateTenantDomainMappingCommand>(c =>
                c.TenantPublicId == tenantPublicId &&
                c.Host == "coffee.chenlis.com" &&
                c.DomainType == "TenantPortal" &&
                c.IsPrimary &&
                c.IsActive),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Should_Send_Command()
    {
        var domainPublicId = Guid.NewGuid();

        var request = new UpdateTenantDomainMappingRequest
        {
            Host = "admin.coffee.chenlis.com",
            DomainType = "TenantAdmin",
            IsPrimary = false,
            IsActive = true
        };

        _mediator
            .Setup(x => x.Send(
                It.IsAny<UpdateTenantDomainMappingCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TenantDomainMappingDto>.Ok(
                new TenantDomainMappingDto
                {
                    DomainPublicId = domainPublicId,
                    TenantPublicId = Guid.NewGuid(),
                    Host = "admin.coffee.chenlis.com",
                    DomainType = "TenantAdmin",
                    IsPrimary = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                "Platform.TenantDomain.UpdateSuccess",
                "ok"));

        var controller = new TenantDomainMappingController(_mediator.Object);

        var result = await controller.UpdateAsync(domainPublicId, request, CancellationToken.None);

        result.Should().BeAssignableTo<IActionResult>();

        _mediator.Verify(x => x.Send(
            It.Is<UpdateTenantDomainMappingCommand>(c =>
                c.DomainPublicId == domainPublicId &&
                c.Host == "admin.coffee.chenlis.com" &&
                c.DomainType == "TenantAdmin" &&
                !c.IsPrimary &&
                c.IsActive),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetActiveAsync_Should_Send_Command()
    {
        var domainPublicId = Guid.NewGuid();

        var request = new SetTenantDomainMappingActiveRequest
        {
            IsActive = false
        };

        _mediator
            .Setup(x => x.Send(
                It.IsAny<SetTenantDomainMappingActiveCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TenantDomainMappingDto>.Ok(
                new TenantDomainMappingDto
                {
                    DomainPublicId = domainPublicId,
                    TenantPublicId = Guid.NewGuid(),
                    Host = "coffee.chenlis.com",
                    DomainType = "TenantPortal",
                    IsPrimary = true,
                    IsActive = false,
                    CreatedAt = DateTime.UtcNow
                },
                "Platform.TenantDomain.SetActiveSuccess",
                "ok"));

        var controller = new TenantDomainMappingController(_mediator.Object);

        var result = await controller.SetActiveAsync(domainPublicId, request, CancellationToken.None);

        result.Should().BeAssignableTo<IActionResult>();

        _mediator.Verify(x => x.Send(
            It.Is<SetTenantDomainMappingActiveCommand>(c =>
                c.DomainPublicId == domainPublicId &&
                c.IsActive == false),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Should_Send_Command()
    {
        var domainPublicId = Guid.NewGuid();

        _mediator
            .Setup(x => x.Send(
                It.IsAny<DeleteTenantDomainMappingCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<bool>.Ok(
                true,
                "Platform.TenantDomain.DeleteSuccess",
                "ok"));

        var controller = new TenantDomainMappingController(_mediator.Object);

        var result = await controller.DeleteAsync(domainPublicId, CancellationToken.None);

        result.Should().BeAssignableTo<IActionResult>();

        _mediator.Verify(x => x.Send(
            It.Is<DeleteTenantDomainMappingCommand>(c => c.DomainPublicId == domainPublicId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}