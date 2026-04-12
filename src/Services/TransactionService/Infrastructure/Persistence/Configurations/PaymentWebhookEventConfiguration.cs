using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransactionService.Domain.Entities;

namespace TransactionService.Infrastructure.Persistence.Configurations;

public class PaymentWebhookEventConfiguration : IEntityTypeConfiguration<PaymentWebhookEvent>
{
    public void Configure(EntityTypeBuilder<PaymentWebhookEvent> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Provider)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.EventId)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.EventType)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(x => new { x.Provider, x.EventId })
            .IsUnique();    }
}