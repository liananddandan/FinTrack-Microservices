using TransactionService.Domain.Constants;

namespace TransactionService.Api.Payments.Contracts;

public class CreatePaymentRequest
{
    public Guid OrderPublicId { get; set; }
    public string Provider { get; set; } = PaymentProviders.Stripe;
    public string PaymentMethod { get; set; } = PaymentMethods.Card;
}