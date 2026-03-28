namespace TransactionService.Api.Transaction.Contracts;

public class TransactionQueryRequest
{
    public string TenantPublicId { get; set; } = string.Empty;

    public string? Type { get; set; }
    public string? Status { get; set; }
    public string? PaymentStatus { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}