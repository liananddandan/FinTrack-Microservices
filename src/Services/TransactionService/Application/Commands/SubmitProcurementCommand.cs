using MediatR;
using SharedKernel.Common.Results;

namespace TransactionService.Application.Commands;

public record SubmitProcurementCommand(
    string TenantPublicId,
    string UserPublicId,
    string TransactionPublicId
) : IRequest<ServiceResult<bool>>;