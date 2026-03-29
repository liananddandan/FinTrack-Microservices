using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Orders.Dtos;

namespace TransactionService.Application.Orders.Queries;


public record GetOrderSummaryQuery(
    bool CreatedByMe,
    DateTime? FromUtc,
    DateTime? ToUtc
) : IRequest<ServiceResult<OrderSummaryDto>>;