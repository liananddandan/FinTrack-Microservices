using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Orders.Dtos;

namespace TransactionService.Application.Orders.Queries;

public record GetOrderByPublicIdQuery(Guid OrderPublicId)
    : IRequest<ServiceResult<OrderDto>>;