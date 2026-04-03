using FluentAssertions;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Tests.UnitTests.Tenant;

public class TenantRepositoryTests
{
    private class TestDbContext : ApplicationIdentityDbContext
    {
        public TestDbContext(DbContextOptions<ApplicationIdentityDbContext> options) : base(options) { }
    }

    [Fact]
    public async Task AddTenantAsync_Should_Save_Tenant()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationIdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new TestDbContext(options);
        var repo = new TenantRepository(dbContext);
        var guid = Guid.NewGuid();
        var tenant = new Domain.Entities.Tenant()
        {
            Id = 1001,
            PublicId = guid,
            Name = "Test Tenant Name"
        };
        
        // Act
        await repo.AddTenantAsync(tenant, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        
        // Assert
        var result = await dbContext.Tenants.SingleOrDefaultAsync(t => t.Id == tenant.Id, CancellationToken.None);
        result.Should().NotBeNull();
        result.Name.Should().Be(tenant.Name);
        result.PublicId.Should().Be(tenant.PublicId);
    }

    [Fact]
    public async Task GetTenantByPublicIdAsync_ShouldReturnNull_WhenTenantNotFound()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationIdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new TestDbContext(options);
        var sut = new TenantRepository(dbContext);
        
        // Act
        var result = await sut.GetTenantByPublicIdAsync("publicId", CancellationToken.None);
        
        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task GetTenantByPublicIdAsync_ShouldReturnTenant_WhenTenantExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationIdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new TestDbContext(options);
        var sut = new TenantRepository(dbContext);
        var tenant = new Domain.Entities.Tenant()
        {
            Name = "Tenant_Get_Tenant_By_PublicId"
        };
        dbContext.Tenants.Add(tenant);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        
        // Act
        var result = await sut.GetTenantByPublicIdAsync(tenant.PublicId.ToString(), CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(tenant);
    }
}