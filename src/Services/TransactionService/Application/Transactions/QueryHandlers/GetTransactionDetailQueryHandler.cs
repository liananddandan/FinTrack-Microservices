using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Api.Transaction.Contracts;
using TransactionService.Application.Transactions.Abstractions;
using TransactionService.Application.Transactions.Queries;

namespace TransactionService.Application.Transactions.QueryHandlers;

public class GetTransactionDetailQueryHandler(
    ITransactionService transactionService
) : IRequestHandler<GetTransactionDetailQuery, ServiceResult<TransactionDetailDto>>
{
    public async Task<ServiceResult<TransactionDetailDto>> Handle(
        GetTransactionDetailQuery request,
        CancellationToken cancellationToken)
    {
        return await transactionService.GetTransactionDetailAsync(
            request.TenantPublicId,
            request.UserPublicId,
            request.Role,
            request.TransactionPublicId,
            cancellationToken);
    }
}