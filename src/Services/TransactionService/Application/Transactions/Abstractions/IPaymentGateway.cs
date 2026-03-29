using TransactionService.Api.Transaction.Contracts;

namespace TransactionService.Application.Transactions.Abstractions;

public interface IPaymentGateway
{
    Task<PaymentExecutionResult> PayAsync(
        PaymentExecutionRequest request,
        CancellationToken cancellationToken = default);
}