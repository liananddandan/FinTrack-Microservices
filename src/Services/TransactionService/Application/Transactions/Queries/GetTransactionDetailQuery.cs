using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Api.Transaction.Contracts;

namespace TransactionService.Application.Transactions.Queries;

public record GetTransactionDetailQuery(
    string TenantPublicId,
    string UserPublicId,
    string Role,
    string TransactionPublicId
) : IRequest<ServiceResult<TransactionDetailDto>>;