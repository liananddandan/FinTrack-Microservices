using TransactionService.Api.Contracts;
using TransactionService.Application.Common.DTOs;

namespace TransactionService.Application.Abstractions;

public interface IPaymentGateway
{
    Task<PaymentExecutionResult> PayAsync(
        PaymentExecutionRequest request,
        CancellationToken cancellationToken = default);
}