using AutoFixture.Xunit2;
using FluentAssertions;
using IdentityService.Common.Status;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Repositories;
using IdentityService.Tests.Attributes;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Tests.Repositories;

public class TenantInvitationRepoTests
{
    private class TestDbContext : ApplicationIdentityDbContext
    {
        public TestDbContext(DbContextOptions<ApplicationIdentityDbContext> options) : base(options) { }
    }

    [Theory, AutoMoqData]
    public async Task AddAsync_ShouldAddTenantInvitation(
        TenantInvitation invitation)
    {
        var options = new DbContextOptionsBuilder<ApplicationIdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var dbContext = new TestDbContext(options);
        var repo = new TenantInvitationRepo(dbContext);

        // Act
        await repo.AddAsync(invitation);
        await dbContext.SaveChangesAsync();
        
        // Assert
        var result = await dbContext.TenantInvitations
            .Where(i => i.Email == invitation.Email)
            .FirstOrDefaultAsync();
        result.Should().NotBeNull();
        result.Email.Should().Be(invitation.Email);
        result.PublicId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task FindByPublicIdAsync_ShouldReturnNull_WhenInvitationNotExist()
    {
        var options = new DbContextOptionsBuilder<ApplicationIdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var dbContext = new TestDbContext(options);
        var repo = new TenantInvitationRepo(dbContext);
        
        // Act
        var result = await repo.FindByPublicIdAsync(Guid.NewGuid());
        
        // Assert
        result.Should().BeNull();
    }
    
    [Theory, AutoMoqData]
    public async Task FindByPublicIdAsync_ShouldReturnInvitation_WhenInvitationExist(
        TenantInvitation invitation)
    {
        var options = new DbContextOptionsBuilder<ApplicationIdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var dbContext = new TestDbContext(options);
        var repo = new TenantInvitationRepo(dbContext);
        dbContext.TenantInvitations.Add(invitation);
        await dbContext.SaveChangesAsync();
        
        // Act
        var result = await repo.FindByPublicIdAsync(invitation.PublicId);
        
        // Assert
        result.Should().NotBeNull();
    }
    
    [Fact]
    public async Task FindByEmailAsync_ShouldReturnNull_WhenInvitationNotExist()
    {
        var options = new DbContextOptionsBuilder<ApplicationIdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var dbContext = new TestDbContext(options);
        var repo = new TenantInvitationRepo(dbContext);
        
        // Act
        var result = await repo.FindByEmailAsync("email");
        
        // Assert
        result.Should().BeNull();
    }
    
    [Theory, AutoMoqData]
    public async Task FindByEmailAsync_ShouldReturnInvitation_WhenInvitationExist(
        TenantInvitation invitation)
    {
        var options = new DbContextOptionsBuilder<ApplicationIdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var dbContext = new TestDbContext(options);
        var repo = new TenantInvitationRepo(dbContext);
        dbContext.TenantInvitations.Add(invitation);
        await dbContext.SaveChangesAsync();
        
        // Act
        var result = await repo.FindByEmailAsync(invitation.Email);
        
        // Assert
        result.Should().NotBeNull();
    }
    
    [Theory, AutoMoqData]
    public async Task UpdateAsync_ShouldReturnUpdatedInvitation(
        TenantInvitation invitation)
    {
        var options = new DbContextOptionsBuilder<ApplicationIdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var dbContext = new TestDbContext(options);
        var repo = new TenantInvitationRepo(dbContext);
        dbContext.TenantInvitations.Add(invitation);
        await dbContext.SaveChangesAsync();
        
        var originalVersion = invitation.Version;
        invitation.Version++;
        invitation.Status = InvitationStatus.Accepted;
        // Act
        await repo.UpdateAsync(invitation);
        await dbContext.SaveChangesAsync();
        
        // Assert
        var result = await dbContext.TenantInvitations
            .Where(ti => ti.PublicId == invitation.PublicId)
            .FirstOrDefaultAsync();
        result.Should().NotBeNull();
        result.Version.Should().Be(originalVersion + 1);
        result.Status.Should().Be(InvitationStatus.Accepted);
    }
}