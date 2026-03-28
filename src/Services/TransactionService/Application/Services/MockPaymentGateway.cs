using TransactionService.Api.Contracts;
using TransactionService.Application.Abstractions;
using TransactionService.Application.Common.DTOs;

namespace TransactionService.Application.Services;

public class MockPaymentGateway : IPaymentGateway
{
    public Task<PaymentExecutionResult> PayAsync(
        PaymentExecutionRequest request,
        CancellationToken cancellationToken = default)
    {
        var success = request.Amount <= 10000;

        if (success)
        {
            return Task.FromResult(new PaymentExecutionResult
            {
                Success = true,
                PaymentReference = $"MOCK-PAY-{DateTime.UtcNow:yyyyMMddHHmmssfff}"
            });
        }

        return Task.FromResult(new PaymentExecutionResult
        {
            Success = false,
            FailureReason = "Mock payment rejected because amount exceeds the allowed threshold."
        });
    }
}