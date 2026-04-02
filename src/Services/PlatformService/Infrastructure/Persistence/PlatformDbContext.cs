using Microsoft.EntityFrameworkCore;
using PlatformService.Domain.Entities;

namespace PlatformService.Infrastructure.Persistence;

public class PlatformDbContext(DbContextOptions<PlatformDbContext> options) : DbContext(options)
{
    public DbSet<TenantDomainMapping> TenantDomainMappings => Set<TenantDomainMapping>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        base.OnModelCreating(builder);
    }
}