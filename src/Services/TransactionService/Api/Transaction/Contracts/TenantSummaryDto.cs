namespace TransactionService.Api.Transaction.Contracts;

public class TenantSummaryDto
{
    public string TenantPublicId { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
}