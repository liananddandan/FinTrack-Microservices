namespace TransactionService.Application.Orders.Dtos;

public class OrderItemDto
{
    public Guid ProductPublicId { get; set; }
    public string ProductNameSnapshot { get; set; } = default!;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
    public string? Notes { get; set; }
}

public class OrderDto
{
    public Guid PublicId { get; set; }
    public string OrderNumber { get; set; } = default!;

    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }

    public Guid CreatedByUserPublicId { get; set; }
    public string CreatedByUserNameSnapshot { get; set; } = default!;

    public decimal SubtotalAmount { get; set; }
    public decimal GstRate { get; set; }
    public decimal GstAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = default!;
    public string PaymentStatus { get; set; } = default!;
    public string PaymentMethod { get; set; } = default!;

    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }

    public List<OrderItemDto> Items { get; set; } = [];
}