using IdentityService.Application.Common.DTOs;
using IdentityService.Domain.Entities;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Abstractions;

public interface ITenantInvitationService
{
    Task<ServiceResult<TenantInvitation>> GetTenantInvitationByPublicIdAsync(string publicId, CancellationToken cancellationToken = default);
    Task<ServiceResult<TenantInvitation>> GetTenantInvitationByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> UpdateTenantInvitationAsync(TenantInvitation invitation, CancellationToken cancellationToken = default);
    
    Task<ServiceResult<bool>> CreateInvitationAsync(
        string tenantPublicId,
        string email,
        string role,
        string invitedByUserPublicId,
        CancellationToken cancellationToken = default);
    
    Task<ServiceResult<ResolveTenantInvitationDto>> ResolveInvitationAsync(
        string invitationPublicId,
        string invitationVersion,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> AcceptInvitationAsync(
        string invitationPublicId,
        string invitationVersion,
        CancellationToken cancellationToken = default);
    
    Task<ServiceResult<List<TenantInvitationDto>>> GetTenantInvitationsAsync(
        string tenantPublicId,
        CancellationToken cancellationToken = default);
    
    Task<ServiceResult<bool>> ResendInvitationAsync(
        string tenantPublicId,
        string invitationPublicId,
        CancellationToken cancellationToken = default);
    
}