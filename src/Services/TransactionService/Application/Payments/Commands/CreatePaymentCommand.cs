namespace TransactionService.Application.Payments.Commands;

public record CreatePaymentCommand(
    Guid OrderPublicId,
    string Provider,
    string PaymentMethod);