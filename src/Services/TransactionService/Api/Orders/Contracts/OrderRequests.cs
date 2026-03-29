namespace TransactionService.Api.Orders.Contracts;

public record CreateOrderItemRequest(
    Guid ProductPublicId,
    int Quantity,
    string? Notes
);

public record CreateOrderRequest(
    string? CustomerName,
    string? CustomerPhone,
    string PaymentMethod,
    List<CreateOrderItemRequest> Items
);