using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Commands;
using TransactionService.Application.Common.DTOs;
using TransactionService.Application.Services.Interfaces;

namespace TransactionService.Application.CommandHandlers;

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