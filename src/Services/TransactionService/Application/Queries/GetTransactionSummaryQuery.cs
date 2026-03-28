using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Common.DTOs;

namespace TransactionService.Application.Queries;

public record GetTransactionSummaryQuery(
    string TenantPublicId,
    string Role
) : IRequest<ServiceResult<TenantTransactionSummaryDto>>;