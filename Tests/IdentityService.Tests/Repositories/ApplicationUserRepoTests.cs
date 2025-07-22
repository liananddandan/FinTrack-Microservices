using AutoFixture.Xunit2;
using FluentAssertions;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Repositories;
using IdentityService.Tests.Attributes;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Tests.Repositories;

public class ApplicationUserRepoTests
{
    private class TestDbContext : ApplicationIdentityDbContext
    {
        public TestDbContext(DbContextOptions<ApplicationIdentityDbContext> options) : base(options) { }
    }

    [Theory, AutoMoqData]
    public async Task ChangeFirstLoginStatus_ShouldChangeStatus(
        ApplicationUser user)
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationIdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new TestDbContext(options);
        var repo = new ApplicationUserRepo(dbContext);
        user.IsFirstLogin = true;
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        var userBefore = await dbContext.Users
            .Where(u => u.PublicId == user.PublicId).FirstOrDefaultAsync();
        userBefore.Should().NotBeNull();
        userBefore.IsFirstLogin.Should().BeTrue();
        
        // Act
        await repo.ChangeFirstLoginStatus(userBefore, CancellationToken.None);
        await dbContext.SaveChangesAsync();
        
        // Assert
        var userAfter = await dbContext.Users
            .AsNoTracking()
            .Where(u => u.PublicId == user.PublicId).FirstOrDefaultAsync();
        
        userAfter.Should().NotBeNull();
        userAfter.IsFirstLogin.Should().BeFalse();
    }

    [Theory, AutoMoqData]
    public async Task IncreaseJwtVersion_ShouldPlusOne(
        ApplicationUser user)
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationIdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new TestDbContext(options);
        var repo = new ApplicationUserRepo(dbContext);
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();
        var originVersion = user.JwtVersion;
        var userBefore = await dbContext.Users
            .Where(u => u.PublicId == user.PublicId)
            .FirstOrDefaultAsync();
        userBefore.Should().NotBeNull();
        userBefore.JwtVersion.Should().Be(originVersion);
        
        // Act
        await repo.IncreaseJwtVersion(userBefore, CancellationToken.None);
        await dbContext.SaveChangesAsync();
        
        // Assert
        var userAfter = await dbContext.Users
            .AsNoTracking()
            .Where(u => u.PublicId == user.PublicId)
            .FirstOrDefaultAsync();
        
        userAfter.Should().NotBeNull();
        userAfter.JwtVersion.Should().Be(originVersion + 1);
    }
}