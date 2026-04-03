using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations;


public class TenantDomainProjectionConfiguration : IEntityTypeConfiguration<TenantDomainProjection>
{
    public void Configure(EntityTypeBuilder<TenantDomainProjection> builder)
    {
        builder.ToTable("TenantDomainProjections");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DomainPublicId).IsRequired();
        builder.Property(x => x.TenantPublicId).IsRequired();

        builder.Property(x => x.Host)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.DomainType)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.IsPrimary).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.LastSyncedAtUtc).IsRequired();

        builder.HasIndex(x => x.DomainPublicId).IsUnique();
        builder.HasIndex(x => x.Host).IsUnique();
    }
}