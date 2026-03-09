using SharedKernel.Common.Results;
using TransactionService.Application.Common.DTOs;
using TransactionService.Application.Services.Interfaces;

namespace TransactionService.Application.Services;

public class MockTenantInfoClient : ITenantInfoClient
{
    public Task<ServiceResult<TenantSummaryDto>> GetTenantSummaryAsync(
        string tenantPublicId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantPublicId))
        {
            return Task.FromResult(ServiceResult<TenantSummaryDto>.Fail(
                ResultCodes.Transaction.TransactionNotFound,
                "Tenant public id is required."));
        }

        var result = new TenantSummaryDto
        {
            TenantPublicId = tenantPublicId,
            TenantName = $"Tenant-{tenantPublicId[..8]}"
        };

        return Task.FromResult(ServiceResult<TenantSummaryDto>.Ok(
            result,
            ResultCodes.Transaction.TransactionQuerySuccess,
            "Tenant summary resolved successfully."));
    }
}