using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Common.Abstractions;
using TransactionService.Application.Payments.Abstractions;
using TransactionService.Application.Payments.Dtos;
using TransactionService.Application.Payments.Queries;
using TransactionService.Infrastructure.Authentication;

namespace TransactionService.Application.Payments.Handlers;

public class GetPaymentsByOrderPublicIdQueryHandler(IPaymentService paymentService)
    : IRequestHandler<GetPaymentsByOrderPublicIdQuery, ServiceResult<List<PaymentListItemDto>>>
{
    public async Task<ServiceResult<List<PaymentListItemDto>>> Handle(
        GetPaymentsByOrderPublicIdQuery request,
        CancellationToken cancellationToken)
    {
        return await paymentService.GetPaymentsByOrderPublicIdAsync(
            request.OrderPublicId,
            cancellationToken);
    }
}