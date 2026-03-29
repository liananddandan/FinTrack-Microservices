using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Api.Transaction.Contracts;

namespace TransactionService.Application.Transactions.Commands;

public record CreateDonationCommand(
    string TenantPublicId,
    string CreatedByUserPublicId,
    string Title,
    string? Description,
    decimal Amount,
    string Currency
) : IRequest<ServiceResult<CreateTransactionResult>>;