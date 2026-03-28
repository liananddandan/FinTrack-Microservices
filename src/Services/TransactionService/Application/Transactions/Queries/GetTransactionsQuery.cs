using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Api.Transaction.Contracts;

namespace TransactionService.Application.Transactions.Queries;

public record GetTransactionsQuery(
    string TenantPublicId,
    string Role,
    string? Type,
    string? Status,
    string? PaymentStatus,
    int PageNumber,
    int PageSize
) : IRequest<ServiceResult<PagedResult<TransactionListItemDto>>>;