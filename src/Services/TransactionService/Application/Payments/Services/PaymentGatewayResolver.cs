using TransactionService.Application.Payments.Abstractions;

namespace TransactionService.Application.Payments.Services;

public class PaymentGatewayResolver(IEnumerable<IPaymentGateway> gateways) : IPaymentGatewayResolver
{
    public IPaymentGateway Resolve(string provider)
    {
        var gateway = gateways.FirstOrDefault(x =>
            string.Equals(x.Provider, provider, StringComparison.OrdinalIgnoreCase));

        if (gateway is null)
        {
            throw new InvalidOperationException($"Unsupported payment provider: {provider}");
        }

        return gateway;
    }
}