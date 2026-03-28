using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Abstractions;
using TransactionService.Application.Commands;

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