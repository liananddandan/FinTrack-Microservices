using IdentityService.Common.DTOs;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;

namespace IdentityService.Services.Interfaces;

public interface ITenantService
{
    Task<ServiceResult<RegisterTenantResult>> RegisterTenantAsync(string tenantName, string adminName,
        string adminEmail, CancellationToken cancellationToken = default);
    
    Task<ServiceResult<bool>> InviteUserForTenantAsync(string adminPublicId,
        string tenantPublicId, string adminRoleInTenant, List<string> emails, CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> ReceiveInviteForTenantAsync(string invitationPublicId, CancellationToken cancellationToken = default);
    
    Task<ServiceResult<IEnumerable<UserInfoDto>>> GetUsersForTenantAsync(string adminPublicId, 
        string tenantPublicId, string adminRoleInTenant, CancellationToken cancellationToken = default);
}