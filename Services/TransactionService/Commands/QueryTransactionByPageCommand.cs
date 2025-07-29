using MediatR;
using SharedKernel.Common.Results;
using TransactionService.Common.DTOs;

namespace TransactionService.Commands;

public record QueryTransactionByPageCommand(
    string TenantPublicId,
    string UserPublicId,
    DateTime? StartDate,
    DateTime? EndDate,
    int Page,
    int PageSize,
    string SortBy
    ): IRequest<ServiceResult<QueryByPageDto>>;