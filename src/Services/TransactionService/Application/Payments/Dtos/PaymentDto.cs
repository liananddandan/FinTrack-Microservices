namespace TransactionService.Application.Payments.Dtos;

public class PaymentDto
{
    public Guid PaymentPublicId { get; set; }
    public Guid OrderPublicId { get; set; }

    public string Provider { get; set; } = default!;
    public string PaymentMethod { get; set; } = default!;
    public string Status { get; set; } = default!;

    public decimal Amount { get; set; }
    public string Currency { get; set; } = default!;

    public string? ProviderPaymentReference { get; set; }
    public string? ProviderClientSecret { get; set; }
    public string? FailureReason { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? RefundedAt { get; set; }
}