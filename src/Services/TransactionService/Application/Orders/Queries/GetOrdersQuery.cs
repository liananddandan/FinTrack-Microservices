using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Common.Dtos;
using TransactionService.Application.Orders.Dtos;

namespace TransactionService.Application.Orders.Queries;

public record GetOrdersQuery(
    bool CreatedByMe,
    string? Status,
    DateTime? FromUtc,
    DateTime? ToUtc,
    int PageNumber,
    int PageSize
) : IRequest<ServiceResult<PagedResult<OrderListItemDto>>>;