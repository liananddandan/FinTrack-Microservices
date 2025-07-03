using IdentityService.Common.Results;
using IdentityService.DTOs;

namespace IdentityService.Services.Interfaces;

public interface ITenantService
{
    Task<ServiceResult<RegisterTenantResult>> RegisterTenantAsync(string tenantName, string adminName,
        string adminEmail, CancellationToken cancellationToken = default);
}