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
    
    Task<ServiceResult<bool>> InviteUserForTenantAsync(string adminPublicId,
        string tenantPublicId, string adminRoleInTenant, List<string> emails, CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> ReceiveInviteForTenantAsync(string invitationPublicId, CancellationToken cancellationToken = default);
}