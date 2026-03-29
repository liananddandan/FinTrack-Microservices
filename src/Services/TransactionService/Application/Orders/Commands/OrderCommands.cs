using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Orders.Dtos;

namespace TransactionService.Application.Orders.Commands;


public record CreateOrderItemCommand(
    Guid ProductPublicId,
    int Quantity,
    string? Notes
);

public record CreateOrderCommand(
    string? CustomerName,
    string? CustomerPhone,
    string PaymentMethod,
    List<CreateOrderItemCommand> Items
) : IRequest<ServiceResult<OrderDto>>;