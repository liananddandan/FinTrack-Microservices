using PlatformService.Domain.Enums;

namespace PlatformService.Domain.Entities;

public class TenantDomainMapping
{
    public long Id { get; set; }

    public Guid PublicId { get; set; } = Guid.NewGuid();

    public Guid TenantPublicId { get; set; }

    public string Host { get; set; } = string.Empty;

    public TenantDomainType DomainType { get; set; }

    public bool IsPrimary { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}