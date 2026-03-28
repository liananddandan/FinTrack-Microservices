namespace TransactionService.Api.Transaction.Contracts;

public class TenantTransactionSummaryDto
{
    public string TenantPublicId { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;

    public decimal CurrentBalance { get; set; }
    public decimal TotalDonationAmount { get; set; }
    public decimal TotalProcurementAmount { get; set; }

    public int TotalTransactionCount { get; set; }
}