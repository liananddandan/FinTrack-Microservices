using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Abstractions;
using TransactionService.Application.Common.DTOs;
using TransactionService.Application.Queries;

namespace TransactionService.Application.QueryHandlers;

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