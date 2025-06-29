using Microsoft.EntityFrameworkCore;

namespace EmailService.Infrastructure.Persistence;

public class CapDbContext : DbContext
{
    public CapDbContext(DbContextOptions<CapDbContext> options) : base(options) { }
}