using TransactionService.Application.Payments.Abstractions;
using TransactionService.Application.Payments.Dtos;
using TransactionService.Domain.Constants;

namespace TransactionService.Infrastructure.Payments;

public class CashPaymentGateway : IPaymentGateway
{
    public string Provider => PaymentProviders.Cash;

    public Task<CreatePaymentGatewayResult> CreatePaymentAsync(
        CreatePaymentGatewayRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new CreatePaymentGatewayResult
        {
            ProviderPaymentReference = $"cash-{request.OrderPublicId:N}",
            ClientSecret = null,
            Status = PaymentStatuses.Paid
        });
    }
}