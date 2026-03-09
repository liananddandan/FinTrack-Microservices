namespace TransactionService.Infrastructure.Persistence.Repositories.Models;

public class TenantTransactionSummaryModel
{
    public string TenantName { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public decimal TotalDonationAmount { get; set; }
    public decimal TotalProcurementAmount { get; set; }
    public int TotalTransactionCount { get; set; }
}