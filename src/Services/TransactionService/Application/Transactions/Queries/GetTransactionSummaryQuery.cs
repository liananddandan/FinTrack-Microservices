using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Api.Transaction.Contracts;

namespace TransactionService.Application.Transactions.Queries;

public record GetTransactionSummaryQuery(
    string TenantPublicId,
    string Role
) : IRequest<ServiceResult<TenantTransactionSummaryDto>>;