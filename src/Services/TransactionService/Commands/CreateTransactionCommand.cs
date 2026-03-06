using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Common.Responses;

namespace TransactionService.Commands;

public record CreateTransactionCommand(
    string TenantPublicId,
    string UserPublicId,
    decimal Amount,
    string Currency,
    string? Description) : IRequest<ServiceResult<CreateTransactionResponse>>;