namespace TransactionService.Application.Orders.Dtos;


public class OrderListItemDto
{
    public Guid PublicId { get; set; }
    public string OrderNumber { get; set; } = default!;
    public string? CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = default!;
    public string PaymentStatus { get; set; } = default!;
    public string PaymentMethod { get; set; } = default!;
    public string CreatedByUserNameSnapshot { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}