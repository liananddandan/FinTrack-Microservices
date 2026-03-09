using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Common.DTOs;

namespace TransactionService.Application.Commands;

public record CreateProcurementCommand(
    string TenantPublicId,
    string UserPublicId,
    CreateProcurementRequest Request
) : IRequest<ServiceResult<CreateTransactionResult>>;