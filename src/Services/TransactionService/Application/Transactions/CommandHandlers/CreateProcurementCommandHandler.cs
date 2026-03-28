using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Api.Transaction.Contracts;
using TransactionService.Application.Transactions.Abstractions;
using TransactionService.Application.Transactions.Commands;

namespace TransactionService.Application.Transactions.CommandHandlers;

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