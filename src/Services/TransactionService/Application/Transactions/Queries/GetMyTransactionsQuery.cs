using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Api.Transaction.Contracts;

namespace TransactionService.Application.Transactions.Queries;

public record GetMyTransactionsQuery(
    string TenantPublicId,
    string UserPublicId,
    int PageNumber,
    int PageSize
) : IRequest<ServiceResult<PagedResult<TransactionListItemDto>>>;