namespace SharedKernel.Contracts.Platform;

public class TenantDomainUpsertedMessage
{
    public Guid DomainPublicId { get; set; }

    public Guid TenantPublicId { get; set; }

    public string Host { get; set; } = string.Empty;

    public string DomainType { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public bool IsActive { get; set; }

    public DateTime OccurredAtUtc { get; set; }
}