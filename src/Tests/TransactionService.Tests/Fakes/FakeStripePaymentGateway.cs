using TransactionService.Application.Payments.Abstractions;
using TransactionService.Application.Payments.Dtos;
using TransactionService.Domain.Constants;

namespace TransactionService.Tests.Fakes;

public class FakeStripePaymentGateway : IPaymentGateway
{
    public string Provider => PaymentProviders.Stripe;

    public Task<CreatePaymentGatewayResult> CreatePaymentAsync(
        CreatePaymentGatewayRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new CreatePaymentGatewayResult
        {
            ProviderPaymentReference = "pi_fake_test_123",
            ClientSecret = "secret_fake_test_123",
            Status = "requires_payment_method"
        });
    }
}