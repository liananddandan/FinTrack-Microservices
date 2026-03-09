using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Common.DTOs;

namespace TransactionService.Application.Queries;

public record GetTransactionDetailQuery(
    string TenantPublicId,
    string UserPublicId,
    string Role,
    string TransactionPublicId
) : IRequest<ServiceResult<TransactionDetailDto>>;