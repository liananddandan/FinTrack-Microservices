namespace IdentityService.Application.Common.DTOs;

public record ResolveTenantInvitationResult(
    string InvitationPublicId,
    string TenantName,
    string Email,
    string Role,
    string Status,
    DateTime ExpiredAt
);