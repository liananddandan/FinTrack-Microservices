using TransactionService.Application.Payments.Abstractions;

namespace TransactionService.Application.Payments.Services;


public class PaymentProviderResolver(IEnumerable<IPaymentProvider> providers)
    : IPaymentProviderResolver
{
    public IPaymentProvider Resolve(string provider, string paymentMethodType)
    {
        var resolved = providers.FirstOrDefault(x =>
            string.Equals(x.Provider, provider, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.PaymentMethodType, paymentMethodType, StringComparison.OrdinalIgnoreCase));

        if (resolved is null)
        {
            throw new InvalidOperationException(
                $"Payment provider not found. Provider: {provider}, MethodType: {paymentMethodType}");
        }

        return resolved;
    }
}