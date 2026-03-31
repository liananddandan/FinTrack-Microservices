using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Payments.Abstractions;
using TransactionService.Application.Payments.Commands;

namespace TransactionService.Application.Payments.Handlers;

public class HandleStripeWebhookCommandHandler(
    IPaymentService paymentService)
    : IRequestHandler<HandleStripeWebhookCommand, ServiceResult<bool>>
{
    public Task<ServiceResult<bool>> Handle(
        HandleStripeWebhookCommand request,
        CancellationToken cancellationToken)
    {
        return paymentService.HandleStripeWebhookAsync(request, cancellationToken);
    }
}