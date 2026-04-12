namespace TransactionService.Application.Payments.Dtos;

public sealed record CreatePaymentResultDto(
    string PaymentPublicId,
    string Provider,
    string PaymentMethodType,
    string Status,
    string? ClientSecret,
    string? StripeConnectedAccountId);
    
public sealed record PaymentDetailDto(
    string PaymentPublicId,
    string OrderPublicId,
    string Provider,
    string PaymentMethodType,
    string Status,
    string Currency,
    decimal Amount,
    decimal RefundedAmount,
    string? FailureReason,
    DateTime CreatedAt,
    DateTime? PaidAt,
    DateTime? FailedAt,
    DateTime? RefundedAt);
    
public sealed record PaymentListItemDto(
    string PaymentPublicId,
    string Provider,
    string PaymentMethodType,
    string Status,
    string Currency,
    decimal Amount,
    DateTime CreatedAt,
    DateTime? PaidAt);