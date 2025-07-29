using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Common.DTOs;
using TransactionService.Services.Interfaces;

namespace TransactionService.Commands.Handlers;

public class QueryTransactionByPageCommandHandler(ITransactionService transactionService)
    : IRequestHandler<QueryTransactionByPageCommand, ServiceResult<QueryByPageDto>>
{
    public async Task<ServiceResult<QueryByPageDto>> Handle(QueryTransactionByPageCommand request, CancellationToken cancellationToken)
    {
        return await transactionService.QueryTransactionByPageAsync(
            request.TenantPublicId, 
            request.UserPublicId,
            request.StartDate,
            request.EndDate, 
            request.Page, 
            request.PageSize,
            request.SortBy);
    }
}