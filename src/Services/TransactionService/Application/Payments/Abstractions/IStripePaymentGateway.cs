using SharedKernel.Common.Results;
using TransactionService.Application.Payments.Dtos;

namespace TransactionService.Application.Payments.Abstractions;

public interface IStripePaymentGateway
{
    Task<ServiceResult<CreateStripePaymentIntentResult>> CreateCardPaymentIntentAsync(
        decimal amount,
        string currency,
        string connectedAccountId,
        string orderPublicId,
        string paymentPublicId,
        CancellationToken cancellationToken = default);
}