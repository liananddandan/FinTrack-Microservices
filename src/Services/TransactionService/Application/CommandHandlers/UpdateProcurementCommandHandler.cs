using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Commands;
using TransactionService.Application.Services.Interfaces;

namespace TransactionService.Application.CommandHandlers;

public class UpdateProcurementCommandHandler(
    ITransactionService transactionService
) : IRequestHandler<UpdateProcurementCommand, ServiceResult<bool>>
{
    public async Task<ServiceResult<bool>> Handle(
        UpdateProcurementCommand request,
        CancellationToken cancellationToken)
    {
        return await transactionService.UpdateProcurementAsync(
            request.TenantPublicId,
            request.UserPublicId,
            request.TransactionPublicId,
            request.Request,
            cancellationToken);
    }
}