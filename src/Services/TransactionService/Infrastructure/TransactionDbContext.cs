using Microsoft.EntityFrameworkCore;
using TransactionService.Domain.Entities;

namespace TransactionService.Infrastructure;

public class TransactionDbContext : DbContext
{
    public DbSet<Transaction> Transactions { get; set; }
    public TransactionDbContext(DbContextOptions<TransactionDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(TransactionDbContext).Assembly);
    }
}