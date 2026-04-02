using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlatformService.Domain.Entities;

namespace PlatformService.Infrastructure.Persistence.Configurations;

public class TenantDomainMappingConfiguration : IEntityTypeConfiguration<TenantDomainMapping>
{
    public void Configure(EntityTypeBuilder<TenantDomainMapping> builder)
    {
        builder.ToTable("TenantDomainMappings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PublicId)
            .IsRequired();

        builder.Property(x => x.TenantPublicId)
            .IsRequired();

        builder.Property(x => x.Host)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.DomainType)
            .IsRequired();

        builder.Property(x => x.IsPrimary)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.PublicId)
            .IsUnique();

        builder.HasIndex(x => x.Host)
            .IsUnique();

        builder.HasIndex(x => new { x.TenantPublicId, x.DomainType, x.IsPrimary });
    }
}