using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Api.Transaction.Contracts;
using TransactionService.Application.Transactions.Abstractions;
using TransactionService.Application.Transactions.Queries;

namespace TransactionService.Application.Transactions.QueryHandlers;

public class GetTransactionsQueryHandler(
    ITransactionService transactionService
) : IRequestHandler<GetTransactionsQuery, ServiceResult<PagedResult<TransactionListItemDto>>>
{
    public async Task<ServiceResult<PagedResult<TransactionListItemDto>>> Handle(
        GetTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        return await transactionService.GetTransactionsAsync(
            request.TenantPublicId,
            request.Role,
            request.Type,
            request.Status,
            request.PaymentStatus,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}