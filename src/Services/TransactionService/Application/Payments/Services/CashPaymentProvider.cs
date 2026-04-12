using SharedKernel.Common.Results;
using SharedKernel.Contracts.Payments;
using TransactionService.Application.Payments.Abstractions;
using TransactionService.Application.Payments.Dtos;

namespace TransactionService.Application.Payments.Services;

public class CashPaymentProvider : IPaymentProvider
{
    public string Provider => PaymentProviders.Cash;
    public string PaymentMethodType => PaymentMethodTypes.Cash;

    public Task<ServiceResult<CreateProviderPaymentResult>> CreatePaymentAsync(
        CreateProviderPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = new CreateProviderPaymentResult(
            ExternalPaymentId: $"cash_{Guid.NewGuid():N}",
            ExternalChargeId: null,
            ClientSecret: null,
            InitialStatus: PaymentStatuses.Succeeded,
            FailureReason: null);

        return Task.FromResult(
            ServiceResult<CreateProviderPaymentResult>.Ok(
                result,
                "CASH_PAYMENT_CREATED",
                "Cash payment created successfully."));
    }
}