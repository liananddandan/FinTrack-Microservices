using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Payments.Abstractions;
using TransactionService.Application.Payments.Commands;
using TransactionService.Application.Payments.Dtos;

namespace TransactionService.Application.Payments.Handlers;

public class CreatePaymentCommandHandler(IPaymentService paymentService)
    : IRequestHandler<CreatePaymentCommand, ServiceResult<CreatePaymentResultDto>>
{
    public async Task<ServiceResult<CreatePaymentResultDto>> Handle(
        CreatePaymentCommand request,
        CancellationToken cancellationToken)
    {
        return await paymentService.CreatePaymentAsync(
            request.OrderPublicId,
            request.PaymentMethodType,
            cancellationToken);
    }
}