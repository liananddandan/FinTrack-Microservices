using SharedKernel.Common.Results;
using TransactionService.Application.Payments.Commands;
using TransactionService.Application.Payments.Dtos;
using TransactionService.Application.Payments.Queries;

namespace TransactionService.Application.Payments.Abstractions;


public interface IPaymentService
{
    Task<ServiceResult<PaymentDto>> CreateAsync(
        CreatePaymentCommand command,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<PaymentDto>> GetByOrderAsync(
        GetPaymentByOrderQuery query,
        CancellationToken cancellationToken = default);
}