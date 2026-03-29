using MediatR;
using SharedKernel.Common.Results;

namespace TransactionService.Application.Transactions.Commands;

public record SubmitProcurementCommand(
    string TenantPublicId,
    string UserPublicId,
    string TransactionPublicId
) : IRequest<ServiceResult<bool>>;