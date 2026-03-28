using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Api.Contracts;
using TransactionService.Application.Common.DTOs;

namespace TransactionService.Application.Commands;

public record RejectProcurementCommand(
    string TenantPublicId,
    string Role,
    string TransactionPublicId,
    RejectProcurementRequest Request
) : IRequest<ServiceResult<bool>>;