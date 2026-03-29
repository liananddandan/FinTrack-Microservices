using MediatR;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;
using TransactionService.Api.Transaction.Contracts;
using TransactionService.Application.Orders.Abstractions;
using TransactionService.Application.Orders.Dtos;
using TransactionService.Application.Orders.Queries;

namespace TransactionService.Application.Orders.Handlers;

public class GetOrdersQueryHandler(IOrderService orderService)
    : IRequestHandler<GetOrdersQuery, ServiceResult<PagedResult<OrderListItemDto>>>
{
    public Task<ServiceResult<PagedResult<OrderListItemDto>>> Handle(
        GetOrdersQuery request,
        CancellationToken cancellationToken)
    {
        return orderService.GetPagedAsync(request, cancellationToken);
    }
}