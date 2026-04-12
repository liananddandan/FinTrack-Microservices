namespace TransactionService.Application.Payments.Abstractions;

public interface IPaymentProviderResolver
{
    IPaymentProvider Resolve(string provider, string paymentMethodType);
}