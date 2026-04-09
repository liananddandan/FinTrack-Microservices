using IdentityService.Application.Common.DTOs;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Abstractions;

public interface ITenantService
{
    Task<ServiceResult<RegisterTenantDto>> RegisterTenantAsync(
        string tenantName, 
        string adminName,
        string adminEmail,
        string adminPassword,
        string turnstileToken,
        CancellationToken cancellationToken = default);
    
    Task<ServiceResult<List<TenantMemberDto>>> GetTenantMembersAsync(
        string tenantPublicId,
        CancellationToken cancellationToken = default);
    
    Task<ServiceResult<bool>> RemoveTenantMemberAsync(
        string tenantPublicId,
        string membershipPublicId,
        string operatorUserPublicId,
        CancellationToken cancellationToken = default);
    
    Task<ServiceResult<bool>> ChangeTenantMemberRoleAsync(
        string tenantPublicId,
        string membershipPublicId,
        string operatorUserPublicId,
        string role,
        CancellationToken cancellationToken = default);
}