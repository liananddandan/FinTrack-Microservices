namespace TransactionService.Application.Payments.Dtos;

public class CreatePaymentGatewayRequest
{
    public required string IdempotencyKey { get; set; }
    public required Guid OrderPublicId { get; set; }
    public required string OrderNumber { get; set; }
    public required decimal Amount { get; set; }
    public required string Currency { get; set; }
    public required string PaymentMethod { get; set; }
    public required string Description { get; set; }
}