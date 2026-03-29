using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Api.Transaction.Contracts;
using TransactionService.Application.Transactions.Abstractions;
using TransactionService.Application.Transactions.Commands;

namespace TransactionService.Application.Transactions.CommandHandlers;

public class CreateDonationCommandHandler(
    ITransactionService transactionService
) : IRequestHandler<CreateDonationCommand, ServiceResult<CreateTransactionResult>>
{
    public async Task<ServiceResult<CreateTransactionResult>> Handle(
        CreateDonationCommand request,
        CancellationToken cancellationToken)
    {
        return await transactionService.CreateDonationAsync(
            request.TenantPublicId,
            request.CreatedByUserPublicId,
            request.Title,
            request.Description,
            request.Amount,
            request.Currency,
            cancellationToken);
    }
}