using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Abstractions;
using TransactionService.Application.Commands;

namespace TransactionService.Application.CommandHandlers;

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