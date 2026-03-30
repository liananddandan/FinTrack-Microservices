namespace TransactionService.Application.Payments.Queries;

public record GetPaymentByOrderQuery(Guid OrderPublicId);