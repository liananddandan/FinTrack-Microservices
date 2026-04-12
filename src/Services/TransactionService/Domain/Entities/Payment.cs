namespace TransactionService.Domain.Entities;

public class Payment
{
    public long Id { get; set; }
    public Guid PublicId { get; set; } = Guid.NewGuid();

    // Cross-service reference
    public Guid TenantPublicId { get; set; }

    // Local relation inside TransactionService
    public long OrderId { get; set; }
    public Order Order { get; set; } = null!;

    // Public id snapshot for easier querying / returning
    public Guid OrderPublicId { get; set; }

    public string Provider { get; set; } = string.Empty;
    public string PaymentMethodType { get; set; } = string.Empty;
    public string Currency { get; set; } = "NZD";

    public decimal Amount { get; set; }
    public decimal RefundedAmount { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? ProviderPaymentIntentId { get; set; }
    public string? ProviderChargeId { get; set; }
    public string? StripeConnectedAccountId { get; set; }

    public string? FailureReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public DateTime? RefundedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}