using SharedKernel.Common.Results;
using TransactionService.Application.Common.DTOs;

namespace TransactionService.Application.Services.Interfaces;

public interface ITenantInfoClient
{
    Task<ServiceResult<TenantSummaryDto>> GetTenantSummaryAsync(
        string tenantPublicId,
        CancellationToken cancellationToken = default);
}