using IdentityService.Common.DTOs;
using SharedKernel.Common.Results;

namespace IdentityService.Services.Interfaces;

public interface ITenantService
{
    Task<ServiceResult<RegisterTenantResult>> RegisterTenantAsync(string tenantName, string adminName,
        string adminEmail, CancellationToken cancellationToken = default);
    
    Task<ServiceResult<bool>> InviteUserForTenantAsync(string adminPublicId, string adminJwtVersion,
        string tenantPublicId, string adminRoleInTenant, List<string> emails, CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> ReceiveInviteForTenantAsync(string invitationPublicId, 
        string invitationVersion, CancellationToken cancellationToken = default);
}