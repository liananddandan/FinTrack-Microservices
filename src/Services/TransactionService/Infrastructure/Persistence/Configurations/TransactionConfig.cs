using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransactionService.Domain.Entities;

namespace TransactionService.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.PublicId)
            .IsRequired();

        entity.Property(x => x.TenantNameSnapshot)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(x => x.Description)
            .HasMaxLength(1000);

        entity.Property(x => x.Currency)
            .HasMaxLength(10)
            .IsRequired();

        entity.Property(x => x.Amount)
            .HasPrecision(18, 2);

        entity.Property(x => x.PaymentReference)
            .HasMaxLength(100);

        entity.Property(x => x.FailureReason)
            .HasMaxLength(500);

        entity.HasIndex(x => x.PublicId)
            .IsUnique();

        entity.HasIndex(x => new { x.TenantPublicId, x.Type, x.CreatedAtUtc });

        entity.HasIndex(x => new { x.TenantPublicId, x.Status, x.CreatedAtUtc });

        entity.HasIndex(x => new { x.TenantPublicId, x.CreatedByUserPublicId, x.CreatedAtUtc });
    }
}