using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Orders.Abstractions;
using TransactionService.Application.Orders.Dtos;
using TransactionService.Application.Orders.Queries;

namespace TransactionService.Application.Orders.Handlers;

public class GetOrderByPublicIdQueryHandler(IOrderService orderService)
    : IRequestHandler<GetOrderByPublicIdQuery, ServiceResult<OrderDto>>
{
    public Task<ServiceResult<OrderDto>> Handle(
        GetOrderByPublicIdQuery request,
        CancellationToken cancellationToken)
    {
        return orderService.GetByPublicIdAsync(request, cancellationToken);
    }
}