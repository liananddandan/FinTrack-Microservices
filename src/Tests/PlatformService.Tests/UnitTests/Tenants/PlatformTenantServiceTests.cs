using FluentAssertions;
using Moq;
using PlatformService.Application.Tenants.Abstractions;
using PlatformService.Application.Tenants.Dtos;
using PlatformService.Application.Tenants.Services;

namespace PlatformService.Tests.UnitTests.Tenants;

public class PlatformTenantServiceTests
{
    private readonly Mock<IIdentityTenantDirectoryClient> _identityTenantDirectoryClient = new();

    [Fact]
    public async Task GetAllTenantsAsync_Should_Return_Tenants_From_Identity_Client()
    {
        var expected = new List<TenantSummaryDto>
        {
            new()
            {
                TenantPublicId = Guid.NewGuid().ToString(),
                TenantName = "Auckland Coffee",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                TenantPublicId = Guid.NewGuid().ToString(),
                TenantName = "Sushi Bar",
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        _identityTenantDirectoryClient
            .Setup(x => x.GetAllTenantsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var service = new PlatformTenantService(_identityTenantDirectoryClient.Object);

        var result = await service.GetAllTenantsAsync(CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.Data![0].TenantName.Should().Be("Auckland Coffee");
        result.Code.Should().Be("Platform.Tenant.GetAllSuccess");
    }

    [Fact]
    public async Task GetAllTenantsAsync_Should_Return_Empty_List_When_No_Tenants()
    {
        _identityTenantDirectoryClient
            .Setup(x => x.GetAllTenantsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TenantSummaryDto>());

        var service = new PlatformTenantService(_identityTenantDirectoryClient.Object);

        var result = await service.GetAllTenantsAsync(CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEmpty();
    }
}