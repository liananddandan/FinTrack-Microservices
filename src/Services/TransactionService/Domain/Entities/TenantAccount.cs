namespace TransactionService.Domain.Entities;

public class TenantAccount
{
    public long Id { get; set; }
    public Guid PublicId { get; set; } = Guid.NewGuid();

    public Guid TenantPublicId { get; set; }
    public decimal AvailableBalance { get; set; }

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}