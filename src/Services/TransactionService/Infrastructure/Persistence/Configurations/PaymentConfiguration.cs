using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransactionService.Domain.Entities;

namespace TransactionService.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasIndex(x => x.TenantPublicId);
        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => x.OrderPublicId);
        builder.HasIndex(x => x.ProviderPaymentIntentId).IsUnique();

        builder.Property(x => x.Provider)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.PaymentMethodType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ProviderPaymentIntentId)
            .HasMaxLength(200);

        builder.Property(x => x.ProviderChargeId)
            .HasMaxLength(200);

        builder.Property(x => x.StripeConnectedAccountId)
            .HasMaxLength(200);

        builder.Property(x => x.FailureReason)
            .HasMaxLength(1000);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.RefundedAmount)
            .HasPrecision(18, 2);

        builder.HasOne(x => x.Order)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}