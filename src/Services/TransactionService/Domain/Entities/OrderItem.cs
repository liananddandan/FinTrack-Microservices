namespace TransactionService.Domain.Entities;

public class OrderItem
{
    public long Id { get; set; }

    public long OrderId { get; set; }
    public Order Order { get; set; } = default!;

    public Guid ProductPublicId { get; set; }

    public string ProductNameSnapshot { get; set; } = default!;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }

    public string? Notes { get; set; }
}