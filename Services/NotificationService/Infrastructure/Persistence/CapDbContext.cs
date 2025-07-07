using Microsoft.EntityFrameworkCore;

namespace NotificationService.Infrastructure.Persistence;

public class CapDbContext : DbContext
{
    public CapDbContext(DbContextOptions<CapDbContext> options) : base(options) { }
}