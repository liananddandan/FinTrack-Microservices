namespace TransactionService.Application.Common.DTOs;

public class TransactionDetailDto
{
    public string TransactionPublicId { get; set; } = string.Empty;
    public string TenantPublicId { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string RiskStatus { get; set; } = string.Empty;

    public string CreatedByUserPublicId { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }

    public string? ApprovedByUserPublicId { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }

    public string? PaidByUserPublicId { get; set; }
    public DateTime? PaidAtUtc { get; set; }

    public string? PaymentReference { get; set; }
    public string? FailureReason { get; set; }

    public string? RefundedByUserPublicId { get; set; }
    public DateTime? RefundedAtUtc { get; set; }
}