using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Common.DTOs;
using TransactionService.Services.Interfaces;

namespace TransactionService.Commands.Handlers;

public class QueryTransactionCommandHandler(ITransactionService transactionService)
    : IRequestHandler<QueryTransactionCommand, ServiceResult<TransactionDto>>
{
    public async Task<ServiceResult<TransactionDto>> Handle(QueryTransactionCommand request, CancellationToken cancellationToken)
    {
        return await transactionService.QueryUserOwnTransactionByPublicIdAsync(request.TenantPublicId, request.UserPublicId,
            request.TransactionPublicId);
    }
}