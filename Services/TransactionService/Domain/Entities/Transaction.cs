
using TransactionService.Common.Status;

namespace TransactionService.Domain.Entities;

public class Transaction
{
    public long Id { get; set; }
    public Guid TransactionPublicId { get; set; } = Guid.NewGuid();
    public required string TenantPublicId { get; set; }
    public required string UserPublicId { get; set; }
    public decimal Amount { get; set; }
    public required string Currency { get; set; }
    public TransactionStatus TransStatus { get; set; }
    public RiskStatus RiskStatus { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}