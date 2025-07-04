using FluentAssertions;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Tests.Repositories;

public class TenantRepoTests
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
        var repo = new TenantRepo(dbContext);
        var guid = Guid.NewGuid();
        var tenant = new Tenant()
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
}