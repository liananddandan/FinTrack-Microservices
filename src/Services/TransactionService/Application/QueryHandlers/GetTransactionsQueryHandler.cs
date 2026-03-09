using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Common.DTOs;
using TransactionService.Application.Queries;
using TransactionService.Application.Services.Interfaces;

namespace TransactionService.Application.QueryHandlers;

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