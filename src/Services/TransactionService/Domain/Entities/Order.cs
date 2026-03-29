using TransactionService.Domain.Constants;

namespace TransactionService.Domain.Entities;

public class Order
{
    public long Id { get; set; }
    public Guid PublicId { get; set; } = Guid.NewGuid();

    public Guid TenantPublicId { get; set; }

    public string OrderNumber { get; set; } = default!;

    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }

    public Guid CreatedByUserPublicId { get; set; }
    public string CreatedByUserNameSnapshot { get; set; } = default!;

    public decimal SubtotalAmount { get; set; }
    public decimal GstRate { get; set; } = 0.15m;
    public decimal GstAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = OrderStatuses.Completed;
    public string PaymentStatus { get; set; } = PaymentStatuses.Paid;
    public string PaymentMethod { get; set; } = PaymentMethods.Cash;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}