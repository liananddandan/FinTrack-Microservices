using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Common.DTOs;

namespace TransactionService.Application.Commands;

public record CreateDonationCommand(
    string TenantPublicId,
    string CreatedByUserPublicId,
    string Title,
    string? Description,
    decimal Amount,
    string Currency
) : IRequest<ServiceResult<CreateTransactionResult>>;