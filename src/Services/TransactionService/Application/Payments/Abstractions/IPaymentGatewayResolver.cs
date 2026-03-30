namespace TransactionService.Application.Payments.Abstractions;

public interface IPaymentGatewayResolver
{
    IPaymentGateway Resolve(string provider);
}