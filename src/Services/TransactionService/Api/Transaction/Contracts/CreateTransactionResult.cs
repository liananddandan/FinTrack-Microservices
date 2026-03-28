namespace TransactionService.Api.Transaction.Contracts;

public class CreateTransactionResult
{
    public string TransactionPublicId { get; set; } = string.Empty;
    public string TenantPublicId { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;

    public string? PaymentReference { get; set; }
    public string? FailureReason { get; set; }
}