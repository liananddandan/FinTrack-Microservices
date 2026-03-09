using MediatR;
using SharedKernel.Common.Results;

namespace TransactionService.Application.Commands;

public record ApproveProcurementCommand(
    string TenantPublicId,
    string Role,
    string TransactionPublicId
) : IRequest<ServiceResult<bool>>;