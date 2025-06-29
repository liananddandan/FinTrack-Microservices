using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.EFConfigurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasQueryFilter(t => !t.IsDeleted);
        builder.HasIndex(t => t.PublicId).IsUnique();
        builder.Property(t => t.PublicId).IsRequired();
        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.Property(t => t.CreatedAt).IsRequired();

    }
}