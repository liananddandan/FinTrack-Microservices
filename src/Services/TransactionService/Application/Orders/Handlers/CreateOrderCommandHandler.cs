using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Orders.Abstractions;
using TransactionService.Application.Orders.Commands;
using TransactionService.Application.Orders.Dtos;

namespace TransactionService.Application.Orders.Handlers;

public class CreateOrderCommandHandler(IOrderService orderService)
    : IRequestHandler<CreateOrderCommand, ServiceResult<OrderDto>>
{
    public Task<ServiceResult<OrderDto>> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        return orderService.CreateAsync(request, cancellationToken);
    }
}