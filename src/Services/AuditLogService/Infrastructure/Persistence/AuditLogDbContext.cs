using AuditLogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditLogService.Infrastructure.Persistence;
public class AuditLogDbContext(DbContextOptions<AuditLogDbContext> options)
    : DbContext(options)
{
    public DbSet<AuditLogEntry> AuditLogs => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuditLogDbContext).Assembly);
    }
}