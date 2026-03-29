using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Orders.Abstractions;
using TransactionService.Application.Orders.Dtos;
using TransactionService.Application.Orders.Queries;

namespace TransactionService.Application.Orders.Handlers;

public class GetOrderSummaryQueryHandler(IOrderService orderService)
    : IRequestHandler<GetOrderSummaryQuery, ServiceResult<OrderSummaryDto>>
{
    public Task<ServiceResult<OrderSummaryDto>> Handle(
        GetOrderSummaryQuery request,
        CancellationToken cancellationToken)
    {
        return orderService.GetSummaryAsync(request, cancellationToken);
    }
}