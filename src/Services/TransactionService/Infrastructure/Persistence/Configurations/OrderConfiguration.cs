using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransactionService.Domain.Entities;

namespace TransactionService.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(x => x.Id);

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.Property(x => x.PublicId)
            .IsRequired();

        builder.HasIndex(x => x.PublicId)
            .IsUnique();

        builder.Property(x => x.TenantPublicId)
            .IsRequired();

        builder.Property(x => x.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => new { x.TenantPublicId, x.OrderNumber })
            .IsUnique();

        builder.Property(x => x.CustomerName)
            .HasMaxLength(100);

        builder.Property(x => x.CustomerPhone)
            .HasMaxLength(30);

        builder.Property(x => x.CreatedByUserPublicId)
            .IsRequired();

        builder.Property(x => x.CreatedByUserNameSnapshot)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.SubtotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.GstRate)
            .IsRequired()
            .HasPrecision(5, 4);

        builder.Property(x => x.GstAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.DiscountAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.TotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.PaymentStatus)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.PaymentMethod)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.PaidAt);

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.Property(x => x.DeletedAt);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}