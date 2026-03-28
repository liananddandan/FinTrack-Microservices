using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Api.Transaction.Contracts;

namespace TransactionService.Application.Transactions.Commands;

public record RejectProcurementCommand(
    string TenantPublicId,
    string Role,
    string TransactionPublicId,
    RejectProcurementRequest Request
) : IRequest<ServiceResult<bool>>;