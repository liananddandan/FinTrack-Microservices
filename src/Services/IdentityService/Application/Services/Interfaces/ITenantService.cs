using IdentityService.Application.Common.DTOs;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Services.Interfaces;

public interface ITenantService
{
    Task<ServiceResult<RegisterTenantResult>> RegisterTenantAsync(
        string tenantName, 
        string adminName,
        string adminEmail,
        string adminPassword,
        CancellationToken cancellationToken = default);
    
    Task<ServiceResult<List<TenantMemberDto>>> GetTenantMembersAsync(
        string tenantPublicId,
        CancellationToken cancellationToken = default);
    
    Task<ServiceResult<bool>> RemoveTenantMemberAsync(
        string tenantPublicId,
        string membershipPublicId,
        string operatorUserPublicId,
        CancellationToken cancellationToken = default);
}