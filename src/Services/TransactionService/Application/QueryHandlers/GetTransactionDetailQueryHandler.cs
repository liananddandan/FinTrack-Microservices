using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Abstractions;
using TransactionService.Application.Common.DTOs;
using TransactionService.Application.Queries;

namespace TransactionService.Application.QueryHandlers;

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