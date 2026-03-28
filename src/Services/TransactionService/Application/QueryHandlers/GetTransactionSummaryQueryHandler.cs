using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Abstractions;
using TransactionService.Application.Common.DTOs;
using TransactionService.Application.Queries;

namespace TransactionService.Application.QueryHandlers;

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