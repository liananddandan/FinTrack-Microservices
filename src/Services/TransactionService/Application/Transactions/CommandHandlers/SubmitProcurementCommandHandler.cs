using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Transactions.Abstractions;
using TransactionService.Application.Transactions.Commands;

namespace TransactionService.Application.Transactions.CommandHandlers;

public class SubmitProcurementCommandHandler(
    ITransactionService transactionService
) : IRequestHandler<SubmitProcurementCommand, ServiceResult<bool>>
{
    public async Task<ServiceResult<bool>> Handle(
        SubmitProcurementCommand request,
        CancellationToken cancellationToken)
    {
        return await transactionService.SubmitProcurementAsync(
            request.TenantPublicId,
            request.UserPublicId,
            request.TransactionPublicId,
            cancellationToken);
    }
}