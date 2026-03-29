using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Api.Transaction.Contracts;
using TransactionService.Application.Transactions.Abstractions;
using TransactionService.Application.Transactions.Queries;

namespace TransactionService.Application.Transactions.QueryHandlers;

public class GetTransactionSummaryQueryHandler(
    ITransactionService transactionService
) : IRequestHandler<GetTransactionSummaryQuery, ServiceResult<TenantTransactionSummaryDto>>
{
    public async Task<ServiceResult<TenantTransactionSummaryDto>> Handle(
        GetTransactionSummaryQuery request,
        CancellationToken cancellationToken)
    {
        return await transactionService.GetTransactionSummaryAsync(
            request.TenantPublicId,
            request.Role,
            cancellationToken);
    }
}