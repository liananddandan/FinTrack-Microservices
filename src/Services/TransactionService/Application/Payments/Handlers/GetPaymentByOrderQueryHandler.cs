using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Payments.Abstractions;
using TransactionService.Application.Payments.Dtos;
using TransactionService.Application.Payments.Queries;

namespace TransactionService.Application.Payments.Handlers;

public class GetPaymentByOrderQueryHandler(
    IPaymentService paymentService)
    : IRequestHandler<GetPaymentByOrderQuery, ServiceResult<PaymentDto>>
{
    public Task<ServiceResult<PaymentDto>> Handle(
        GetPaymentByOrderQuery request,
        CancellationToken cancellationToken)
    {
        return paymentService.GetByOrderAsync(request, cancellationToken);
    }
}