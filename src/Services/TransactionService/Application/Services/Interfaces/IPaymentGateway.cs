using TransactionService.Application.Common.DTOs;

namespace TransactionService.Application.Services.Interfaces;

public interface IPaymentGateway
{
    Task<PaymentExecutionResult> PayAsync(
        PaymentExecutionRequest request,
        CancellationToken cancellationToken = default);
}