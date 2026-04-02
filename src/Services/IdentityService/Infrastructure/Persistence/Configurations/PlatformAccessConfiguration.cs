using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations;

public class PlatformAccessConfiguration : IEntityTypeConfiguration<PlatformAccess>
{
    public void Configure(EntityTypeBuilder<PlatformAccess> builder)
    {
        builder.ToTable("PlatformAccesses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PublicId)
            .IsRequired();

        builder.Property(x => x.UserPublicId)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Role)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.IsEnabled)
            .IsRequired();

        builder.HasIndex(x => x.UserPublicId)
            .IsUnique();
    }
}