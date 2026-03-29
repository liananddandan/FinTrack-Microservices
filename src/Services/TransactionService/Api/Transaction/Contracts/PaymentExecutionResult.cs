namespace TransactionService.Api.Transaction.Contracts;

public class PaymentExecutionResult
{
    public bool Success { get; set; }
    public string? PaymentReference { get; set; }
    public string? FailureReason { get; set; }
}