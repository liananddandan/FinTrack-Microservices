using MediatR;
using SharedKernel.Common.Results;

namespace TransactionService.Application.Orders.Commands;

public record CancelOrderCommand(Guid OrderPublicId)
    : IRequest<ServiceResult<bool>>;