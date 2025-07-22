using System.Transactions;

namespace TransactionService.Domain.Entities;

public class Transaction
{
    public long Id { get; set; }
    public Guid TransactionPublicId { get; set; }
    public required string TenantPublicId { get; set; }
    public required string UserPublicId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public TransactionStatus Status { get; set; }
}