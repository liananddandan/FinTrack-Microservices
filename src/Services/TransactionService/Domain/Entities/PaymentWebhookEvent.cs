namespace TransactionService.Domain.Entities;

public class PaymentWebhookEvent
{
    public long Id { get; set; }

    public string Provider { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}