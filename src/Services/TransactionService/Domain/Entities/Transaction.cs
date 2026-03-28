using TransactionService.Domain.Enums;

namespace TransactionService.Domain.Entities;

public class Transaction
{
    public long Id { get; set; }
    public Guid PublicId { get; set; } = Guid.NewGuid();

    public Guid TenantPublicId { get; set; }
    public string TenantNameSnapshot { get; set; } = string.Empty;

    public TransactionType Type { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "NZD";

    public TransactionStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public RiskStatus RiskStatus { get; set; }

    public Guid CreatedByUserPublicId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Guid? ApprovedByUserPublicId { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }

    public Guid? PaidByUserPublicId { get; set; }
    public DateTime? PaidAtUtc { get; set; }

    public string? PaymentReference { get; set; }
    public string? FailureReason { get; set; }

    public Guid? RefundedByUserPublicId { get; set; }
    public DateTime? RefundedAtUtc { get; set; }
}