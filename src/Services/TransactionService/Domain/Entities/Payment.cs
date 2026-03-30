using TransactionService.Domain.Constants;

namespace TransactionService.Domain.Entities;

public class Payment
{
    public long Id { get; set; }
    public Guid PublicId { get; set; } = Guid.NewGuid();

    public Guid TenantPublicId { get; set; }

    public long OrderId { get; set; }
    public Order Order { get; set; } = default!;

    public string Provider { get; set; } = default!;
    public string PaymentMethod { get; set; } = PaymentMethods.Card;
    public string Status { get; set; } = PaymentStatuses.NotStarted;

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "NZD";

    public string IdempotencyKey { get; set; } = default!;
    public string? ProviderPaymentReference { get; set; }
    public string? ProviderClientSecret { get; set; }

    public string? FailureReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public DateTime? RefundedAt { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    
}