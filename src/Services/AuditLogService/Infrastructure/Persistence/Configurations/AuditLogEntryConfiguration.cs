using AuditLogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditLogService.Infrastructure.Persistence.Configurations;

public class AuditLogEntryConfiguration : IEntityTypeConfiguration<AuditLogEntry>
{
    public void Configure(EntityTypeBuilder<AuditLogEntry> entity)
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.TenantPublicId)
            .HasMaxLength(64)
            .IsRequired();

        entity.Property(x => x.ActorUserPublicId)
            .HasMaxLength(64);

        entity.Property(x => x.ActorDisplayName)
            .HasMaxLength(256);

        entity.Property(x => x.ActionType)
            .HasMaxLength(128)
            .IsRequired();

        entity.Property(x => x.Category)
            .HasMaxLength(64)
            .IsRequired();

        entity.Property(x => x.TargetType)
            .HasMaxLength(64);

        entity.Property(x => x.TargetPublicId)
            .HasMaxLength(64);

        entity.Property(x => x.TargetDisplay)
            .HasMaxLength(256);

        entity.Property(x => x.Source)
            .HasMaxLength(64);

        entity.Property(x => x.CorrelationId)
            .HasMaxLength(128);

        entity.Property(x => x.IpAddress)
            .HasMaxLength(64);

        entity.Property(x => x.UserAgent)
            .HasMaxLength(512);

        entity.Property(x => x.MetadataJson)
            .HasColumnType("longtext")
            .IsRequired();

        entity.HasIndex(x => new { x.TenantPublicId, x.OccurredAtUtc });
        entity.HasIndex(x => new { x.TenantPublicId, x.ActionType, x.OccurredAtUtc });
        entity.HasIndex(x => new { x.TenantPublicId, x.ActorUserPublicId, x.OccurredAtUtc });
        entity.HasIndex(x => new { x.TenantPublicId, x.TargetPublicId, x.OccurredAtUtc });
    }
}