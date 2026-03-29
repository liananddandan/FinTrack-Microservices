using SharedKernel.Common.Results;
using TransactionService.Api.Transaction.Contracts;
using TransactionService.Application.Orders.Commands;
using TransactionService.Application.Orders.Dtos;
using TransactionService.Application.Orders.Queries;

namespace TransactionService.Application.Orders.Abstractions;

public interface IOrderService
{
    Task<ServiceResult<OrderDto>> CreateAsync(
        CreateOrderCommand request,
        CancellationToken cancellationToken);

    Task<ServiceResult<OrderDto>> GetByPublicIdAsync(
        GetOrderByPublicIdQuery request,
        CancellationToken cancellationToken);

    Task<ServiceResult<PagedResult<OrderListItemDto>>> GetPagedAsync(
        GetOrdersQuery request,
        CancellationToken cancellationToken);

    Task<ServiceResult<bool>> CancelAsync(
        CancelOrderCommand request,
        CancellationToken cancellationToken);
    
    Task<ServiceResult<OrderSummaryDto>> GetSummaryAsync(
        GetOrderSummaryQuery request,
        CancellationToken cancellationToken);
}