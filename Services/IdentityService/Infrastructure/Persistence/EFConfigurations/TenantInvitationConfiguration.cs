using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.EFConfigurations;

public class TenantInvitationConfiguration : IEntityTypeConfiguration<TenantInvitation>
{
    public void Configure(EntityTypeBuilder<TenantInvitation> builder)
    {
        builder.HasIndex(t => t.PublicId).IsUnique();
        builder.Property(t => t.PublicId).IsRequired();
        builder.Property(t => t.Email).HasMaxLength(100).IsRequired();
        builder.Property(t => t.Role).HasMaxLength(100).IsRequired();
        builder.Property(t => t.Version).HasDefaultValue(1);
        builder.Property(t => t.TenantPublicId).IsRequired();
        builder.Property(t => t.CreatedBy).HasMaxLength(100).IsRequired();
    }
}