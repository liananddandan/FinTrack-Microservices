using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Api.Transaction.Contracts;
using TransactionService.Application.Transactions.Abstractions;
using TransactionService.Application.Transactions.Queries;

namespace TransactionService.Application.Transactions.QueryHandlers;

public class GetMyTransactionsQueryHandler(
    ITransactionService transactionService
) : IRequestHandler<GetMyTransactionsQuery, ServiceResult<PagedResult<TransactionListItemDto>>>
{
    public async Task<ServiceResult<PagedResult<TransactionListItemDto>>> Handle(
        GetMyTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        return await transactionService.GetMyTransactionsAsync(
            request.TenantPublicId,
            request.UserPublicId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}