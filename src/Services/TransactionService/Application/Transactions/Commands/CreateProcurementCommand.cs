using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Api.Transaction.Contracts;

namespace TransactionService.Application.Transactions.Commands;

public record CreateProcurementCommand(
    string TenantPublicId,
    string UserPublicId,
    CreateProcurementRequest Request
) : IRequest<ServiceResult<CreateTransactionResult>>;