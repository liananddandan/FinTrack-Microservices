using IdentityService.Domain.Entities;
using SharedKernel.Common.Results;

namespace IdentityService.Services.Interfaces;

public interface ITenantInvitationService
{
    Task<ServiceResult<bool>> AddTenantInvitationAsync(TenantInvitation invitation, CancellationToken cancellationToken = default);
    Task<ServiceResult<TenantInvitation>> GetTenantInvitationByPublicIdAsync(string publicId, CancellationToken cancellationToken = default);
    Task<ServiceResult<TenantInvitation>> GetTenantInvitationByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> UpdateTenantInvitationAsync(TenantInvitation invitation, CancellationToken cancellationToken = default);
}