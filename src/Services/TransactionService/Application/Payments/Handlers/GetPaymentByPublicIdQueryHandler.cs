using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Payments.Abstractions;
using TransactionService.Application.Payments.Dtos;
using TransactionService.Application.Payments.Queries;

namespace TransactionService.Application.Payments.Handlers;

public class GetPaymentByPublicIdQueryHandler(IPaymentService paymentService)
    : IRequestHandler<GetPaymentByPublicIdQuery, ServiceResult<PaymentDetailDto>>
{
    public async Task<ServiceResult<PaymentDetailDto>> Handle(
        GetPaymentByPublicIdQuery request,
        CancellationToken cancellationToken)
    {
        return await paymentService.GetPaymentByPublicIdAsync(
            request.PaymentPublicId,
            cancellationToken);
    }
}