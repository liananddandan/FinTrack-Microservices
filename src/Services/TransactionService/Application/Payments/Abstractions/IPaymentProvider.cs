using SharedKernel.Common.Results;
using TransactionService.Application.Payments.Dtos;

namespace TransactionService.Application.Payments.Abstractions;

public interface IPaymentProvider
{
    string Provider { get; }
    string PaymentMethodType { get; }

    Task<ServiceResult<CreateProviderPaymentResult>> CreatePaymentAsync(
        CreateProviderPaymentRequest request,
        CancellationToken cancellationToken = default);
}