using FluentAssertions;
using Moq;
using PlatformService.Application.Common.Abstractions;
using PlatformService.Application.Tenants.Abstractions;
using PlatformService.Application.Tenants.Commands;
using PlatformService.Application.Tenants.Services;
using PlatformService.Domain.Entities;
using PlatformService.Domain.Enums;

namespace PlatformService.Tests.UnitTests.Tenants;

public class TenantDomainMappingServiceTests
{
    private readonly Mock<ITenantDomainMappingRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private TenantDomainMappingService CreateService()
    {
        return new TenantDomainMappingService(_repository.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task GetByTenantAsync_Should_Return_Mappings()
    {
        var tenantPublicId = Guid.NewGuid();

        var mappings = new List<TenantDomainMapping>
        {
            new()
            {
                PublicId = Guid.NewGuid(),
                TenantPublicId = tenantPublicId,
                Host = "coffee.chenlis.com",
                DomainType = TenantDomainType.TenantPortal,
                IsPrimary = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        _repository
            .Setup(x => x.GetByTenantPublicIdAsync(tenantPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mappings);

        var service = CreateService();

        var result = await service.GetByTenantAsync(tenantPublicId, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(1);
        result.Data[0].Host.Should().Be("coffee.chenlis.com");
        result.Data[0].DomainType.Should().Be("TenantPortal");
    }

    [Fact]
    public async Task CreateAsync_Should_Fail_When_DomainType_Invalid()
    {
        var command = new CreateTenantDomainMappingCommand(
            Guid.NewGuid(),
            "coffee.chenlis.com",
            "WrongType",
            true,
            true);

        var service = CreateService();

        var result = await service.CreateAsync(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("Platform.TenantDomain.InvalidDomainType");
    }

    [Fact]
    public async Task CreateAsync_Should_Fail_When_Host_Already_Exists()
    {
        var tenantPublicId = Guid.NewGuid();

        var command = new CreateTenantDomainMappingCommand(
            tenantPublicId,
            "coffee.chenlis.com",
            "TenantPortal",
            true,
            true);

        _repository
            .Setup(x => x.GetByHostAsync("coffee.chenlis.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TenantDomainMapping
            {
                PublicId = Guid.NewGuid(),
                TenantPublicId = Guid.NewGuid(),
                Host = "coffee.chenlis.com"
            });

        var service = CreateService();

        var result = await service.CreateAsync(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("Platform.TenantDomain.HostAlreadyExists");
    }

    [Fact]
    public async Task CreateAsync_Should_Create_Mapping_Successfully()
    {
        var tenantPublicId = Guid.NewGuid();

        var command = new CreateTenantDomainMappingCommand(
            tenantPublicId,
            "Coffee.Chenlis.com",
            "TenantPortal",
            true,
            true);

        _repository
            .Setup(x => x.GetByHostAsync("coffee.chenlis.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantDomainMapping?)null);

        _repository
            .Setup(x => x.GetPrimaryByTenantAndTypeAsync(
                tenantPublicId,
                TenantDomainType.TenantPortal,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantDomainMapping?)null);

        TenantDomainMapping? added = null;

        _repository
            .Setup(x => x.AddAsync(It.IsAny<TenantDomainMapping>(), It.IsAny<CancellationToken>()))
            .Callback<TenantDomainMapping, CancellationToken>((m, _) => added = m)
            .Returns(Task.CompletedTask);

        var service = CreateService();

        var result = await service.CreateAsync(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Host.Should().Be("coffee.chenlis.com");
        result.Data.DomainType.Should().Be("TenantPortal");

        added.Should().NotBeNull();
        added!.Host.Should().Be("coffee.chenlis.com");
        added.IsPrimary.Should().BeTrue();

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Should_Fail_When_Mapping_Not_Found()
    {
        var command = new UpdateTenantDomainMappingCommand(
            Guid.NewGuid(),
            "coffee.chenlis.com",
            "TenantPortal",
            true,
            true);

        _repository
            .Setup(x => x.GetByPublicIdAsync(command.DomainPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantDomainMapping?)null);

        var service = CreateService();

        var result = await service.UpdateAsync(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("Platform.TenantDomain.NotFound");
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Mapping_Successfully()
    {
        var tenantPublicId = Guid.NewGuid();
        var domainPublicId = Guid.NewGuid();

        var existing = new TenantDomainMapping
        {
            PublicId = domainPublicId,
            TenantPublicId = tenantPublicId,
            Host = "old.chenlis.com",
            DomainType = TenantDomainType.TenantPortal,
            IsPrimary = false,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        var command = new UpdateTenantDomainMappingCommand(
            domainPublicId,
            "new.chenlis.com",
            "TenantPortal",
            true,
            true);

        _repository
            .Setup(x => x.GetByPublicIdAsync(domainPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _repository
            .Setup(x => x.GetByHostAsync("new.chenlis.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantDomainMapping?)null);

        _repository
            .Setup(x => x.GetPrimaryByTenantAndTypeAsync(
                tenantPublicId,
                TenantDomainType.TenantPortal,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantDomainMapping?)null);

        var service = CreateService();

        var result = await service.UpdateAsync(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Host.Should().Be("new.chenlis.com");
        result.Data.IsPrimary.Should().BeTrue();
        result.Data.IsActive.Should().BeTrue();

        existing.Host.Should().Be("new.chenlis.com");
        existing.IsPrimary.Should().BeTrue();
        existing.IsActive.Should().BeTrue();

        _repository.Verify(x => x.Update(existing), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Should_Fail_When_Not_Found()
    {
        var command = new DeleteTenantDomainMappingCommand(Guid.NewGuid());

        _repository
            .Setup(x => x.GetByPublicIdAsync(command.DomainPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantDomainMapping?)null);

        var service = CreateService();

        var result = await service.DeleteAsync(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("Platform.TenantDomain.NotFound");
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Mapping()
    {
        var existing = new TenantDomainMapping
        {
            PublicId = Guid.NewGuid(),
            TenantPublicId = Guid.NewGuid(),
            Host = "coffee.chenlis.com"
        };

        var command = new DeleteTenantDomainMappingCommand(existing.PublicId);

        _repository
            .Setup(x => x.GetByPublicIdAsync(existing.PublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var service = CreateService();

        var result = await service.DeleteAsync(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();

        _repository.Verify(x => x.Remove(existing), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetActiveAsync_Should_Fail_When_Not_Found()
    {
        var command = new SetTenantDomainMappingActiveCommand(Guid.NewGuid(), true);

        _repository
            .Setup(x => x.GetByPublicIdAsync(command.DomainPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantDomainMapping?)null);

        var service = CreateService();

        var result = await service.SetActiveAsync(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("Platform.TenantDomain.NotFound");
    }

    [Fact]
    public async Task SetActiveAsync_Should_Update_Status()
    {
        var existing = new TenantDomainMapping
        {
            PublicId = Guid.NewGuid(),
            TenantPublicId = Guid.NewGuid(),
            Host = "coffee.chenlis.com",
            IsActive = false,
            DomainType = TenantDomainType.TenantPortal,
            CreatedAt = DateTime.UtcNow
        };

        var command = new SetTenantDomainMappingActiveCommand(existing.PublicId, true);

        _repository
            .Setup(x => x.GetByPublicIdAsync(existing.PublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var service = CreateService();

        var result = await service.SetActiveAsync(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.IsActive.Should().BeTrue();

        existing.IsActive.Should().BeTrue();

        _repository.Verify(x => x.Update(existing), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}