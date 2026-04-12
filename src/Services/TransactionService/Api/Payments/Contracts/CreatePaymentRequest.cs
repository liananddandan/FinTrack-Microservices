namespace TransactionService.Api.Payments.Contracts;

public sealed record CreatePaymentRequest(
    string OrderPublicId,
    string PaymentMethodType);