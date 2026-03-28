namespace TransactionService.Application.Common.DTOs;

public class PaymentExecutionRequest
{
    public string TransactionPublicId { get; set; } = string.Empty;
    public string TenantPublicId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}