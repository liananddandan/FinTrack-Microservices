using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Common.Responses;
using TransactionService.Services.Interfaces;

namespace TransactionService.Commands.Handlers;

public class CreateTransactionCommandHandler(
    ITransactionService transactionService
    ): IRequestHandler<CreateTransactionCommand, ServiceResult<CreateTransactionResponse>>
{
    public async Task<ServiceResult<CreateTransactionResponse>> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        return await transactionService.CreateTransactionAsync(
            request.TenantPublicId,
            request.UserPublicId,
            request.Amount,
            request.Currency,
            request.Description);
    }
}