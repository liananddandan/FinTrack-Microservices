using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Application.Common.DTOs;

namespace TransactionService.Application.Queries;

public record GetMyTransactionsQuery(
    string TenantPublicId,
    string UserPublicId,
    int PageNumber,
    int PageSize
) : IRequest<ServiceResult<PagedResult<TransactionListItemDto>>>;