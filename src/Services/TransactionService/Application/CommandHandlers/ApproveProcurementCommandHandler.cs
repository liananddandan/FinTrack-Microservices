using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Abstractions;
using TransactionService.Application.Commands;

namespace TransactionService.Application.CommandHandlers;

public class ApproveProcurementCommandHandler(
    ITransactionService transactionService
) : IRequestHandler<ApproveProcurementCommand, ServiceResult<bool>>
{
    public async Task<ServiceResult<bool>> Handle(
        ApproveProcurementCommand request,
        CancellationToken cancellationToken)
    {
        return await transactionService.ApproveProcurementAsync(
            request.TenantPublicId,
            request.Role,
            request.TransactionPublicId,
            cancellationToken);
    }
}