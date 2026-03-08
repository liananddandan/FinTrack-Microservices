using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Common.DTOs;

namespace TransactionService.Commands;

public record QueryTransactionCommand(string TenantPublicId,
    string UserPublicId,
    string TransactionPublicId) : IRequest<ServiceResult<TransactionDto>>;