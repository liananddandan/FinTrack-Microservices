using IdentityService.Common.DTOs;
using SharedKernel.Common.Results;

namespace IdentityService.Services.Interfaces;

public interface ITenantService
{
    Task<ServiceResult<RegisterTenantResult>> RegisterTenantAsync(string tenantName, string adminName,
        string adminEmail, CancellationToken cancellationToken = default);
}