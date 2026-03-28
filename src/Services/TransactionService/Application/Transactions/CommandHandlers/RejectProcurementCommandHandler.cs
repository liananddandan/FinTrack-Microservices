using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Transactions.Abstractions;
using TransactionService.Application.Transactions.Commands;

namespace TransactionService.Application.Transactions.CommandHandlers;

public class RejectProcurementCommandHandler(
    ITransactionService transactionService
) : IRequestHandler<RejectProcurementCommand, ServiceResult<bool>>
{
    public async Task<ServiceResult<bool>> Handle(
        RejectProcurementCommand request,
        CancellationToken cancellationToken)
    {
        return await transactionService.RejectProcurementAsync(
            request.TenantPublicId,
            request.Role,
            request.TransactionPublicId,
            request.Request,
            cancellationToken);
    }
}