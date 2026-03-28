using SharedKernel.Common.Results;
using TransactionService.Api.Transaction.Contracts;

namespace TransactionService.Application.Transactions.Abstractions;

public interface ITenantInfoClient
{
    Task<ServiceResult<TenantSummaryDto>> GetTenantSummaryAsync(
        string tenantPublicId,
        CancellationToken cancellationToken = default);
}