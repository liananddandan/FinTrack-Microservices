using TransactionService.Application.Payments.Dtos;

namespace TransactionService.Application.Payments.Abstractions;


public interface IPaymentGateway
{
    string Provider { get; }

    Task<CreatePaymentGatewayResult> CreatePaymentAsync(
        CreatePaymentGatewayRequest request,
        CancellationToken cancellationToken = default);
}