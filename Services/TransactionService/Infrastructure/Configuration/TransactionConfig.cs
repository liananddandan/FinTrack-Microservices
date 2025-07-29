using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransactionService.Domain.Entities;

namespace TransactionService.Infrastructure.Configuration;

public class TransactionConfig : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasIndex(t => t.TransactionPublicId).IsUnique();
        builder.Property(t => t.TransactionPublicId).IsRequired();
        builder.Property(t => t.TenantPublicId).HasMaxLength(200).IsRequired();
        builder.Property(t => t.UserPublicId).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Amount)
            .HasColumnType("decimal(18,4)")
            .IsRequired();
    }
}