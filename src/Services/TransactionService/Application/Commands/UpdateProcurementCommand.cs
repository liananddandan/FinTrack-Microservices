using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Common.DTOs;

namespace TransactionService.Application.Commands;

public record UpdateProcurementCommand(
    string TenantPublicId,
    string UserPublicId,
    string TransactionPublicId,
    UpdateProcurementRequest Request
) : IRequest<ServiceResult<bool>>;