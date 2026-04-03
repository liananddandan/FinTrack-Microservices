namespace SharedKernel.Contracts.Platform;

public class TenantDomainRemovedMessage
{
    public Guid DomainPublicId { get; set; }

    public DateTime OccurredAtUtc { get; set; }
}