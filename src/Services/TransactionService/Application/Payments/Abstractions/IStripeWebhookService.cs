using SharedKernel.Common.Results;

namespace TransactionService.Application.Payments.Abstractions;

public interface IStripeWebhookService
{
    Task<ServiceResult<bool>> HandleWebhookAsync(
        string json,
        string signatureHeader,
        CancellationToken cancellationToken = default);
}