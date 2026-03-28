using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Api.Transaction.Contracts;

namespace TransactionService.Application.Transactions.Commands;

public record UpdateProcurementCommand(
    string TenantPublicId,
    string UserPublicId,
    string TransactionPublicId,
    UpdateProcurementRequest Request
) : IRequest<ServiceResult<bool>>;