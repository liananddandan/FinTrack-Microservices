using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransactionService.Domain.Entities;

namespace TransactionService.Infrastructure.Persistence.Configurations;

public class TenantAccountConfiguration : IEntityTypeConfiguration<TenantAccount>
{
    public void Configure(EntityTypeBuilder<TenantAccount> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.PublicId)
            .IsRequired();

        entity.Property(x => x.AvailableBalance)
            .HasPrecision(18, 2);

        entity.HasIndex(x => x.PublicId)
            .IsUnique();

        entity.HasIndex(x => x.TenantPublicId)
            .IsUnique();
    }
}