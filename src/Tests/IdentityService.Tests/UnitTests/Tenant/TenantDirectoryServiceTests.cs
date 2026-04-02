using FluentAssertions;
using IdentityService.Application.Tenants.Abstractions;
using IdentityService.Application.Tenants.Services;
using Moq;

namespace IdentityService.Tests.UnitTests.Tenant;

public class TenantDirectoryServiceTests
{
    private readonly Mock<ITenantRepository> _tenantRepository = new();

    [Fact]
    public async Task GetAllTenantsAsync_Should_Return_Ordered_Tenant_Summaries()
    {
        var service = new TenantDirectoryService(_tenantRepository.Object);

        var tenants = new List<Domain.Entities.Tenant>
        {
            new()
            {
                PublicId = Guid.NewGuid(),
                Name = "Sushi Bar",
                IsActive = true,
                CreatedAt = new DateTime(2026, 4, 2, 10, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                PublicId = Guid.NewGuid(),
                Name = "Auckland Coffee",
                IsActive = false,
                CreatedAt = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc)
            }
        };

        _tenantRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenants);

        var result = await service.GetAllTenantsAsync(CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Count.Should().Be(2);

        result.Data[0].TenantName.Should().Be("Auckland Coffee");
        result.Data[0].IsActive.Should().BeFalse();

        result.Data[1].TenantName.Should().Be("Sushi Bar");
        result.Data[1].IsActive.Should().BeTrue();

        result.Code.Should().Be("Identity.Tenant.GetAllSuccess");
    }

    [Fact]
    public async Task GetAllTenantsAsync_Should_Return_Empty_List_When_No_Tenants()
    {
        var service = new TenantDirectoryService(_tenantRepository.Object);

        _tenantRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.Tenant>());

        var result = await service.GetAllTenantsAsync(CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEmpty();
    }
}