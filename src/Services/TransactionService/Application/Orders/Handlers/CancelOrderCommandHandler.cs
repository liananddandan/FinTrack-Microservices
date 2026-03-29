using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Orders.Abstractions;
using TransactionService.Application.Orders.Commands;

namespace TransactionService.Application.Orders.Handlers;

public class CancelOrderCommandHandler(IOrderService orderService)
    : IRequestHandler<CancelOrderCommand, ServiceResult<bool>>
{
    public Task<ServiceResult<bool>> Handle(
        CancelOrderCommand request,
        CancellationToken cancellationToken)
    {
        return orderService.CancelAsync(request, cancellationToken);
    }
}