using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Commands;
using TransactionService.Application.Services.Interfaces;

namespace TransactionService.Application.CommandHandlers;

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