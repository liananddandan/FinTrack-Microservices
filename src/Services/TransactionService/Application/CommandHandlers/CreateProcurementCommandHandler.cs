using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Commands;
using TransactionService.Application.Common.DTOs;
using TransactionService.Application.Services.Interfaces;

namespace TransactionService.Application.CommandHandlers;

public class CreateProcurementCommandHandler(
    ITransactionService transactionService
) : IRequestHandler<CreateProcurementCommand, ServiceResult<CreateTransactionResult>>
{
    public async Task<ServiceResult<CreateTransactionResult>> Handle(
        CreateProcurementCommand request,
        CancellationToken cancellationToken)
    {
        return await transactionService.CreateProcurementAsync(
            request.TenantPublicId,
            request.UserPublicId,
            request.Request,
            cancellationToken);
    }
}