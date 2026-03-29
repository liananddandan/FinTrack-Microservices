namespace TransactionService.Api.Transaction.Contracts;

public class RejectProcurementRequest
{
    public string Reason { get; set; } = string.Empty;
}