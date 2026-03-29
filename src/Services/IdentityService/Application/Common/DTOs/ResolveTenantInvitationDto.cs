namespace IdentityService.Application.Common.DTOs;

public record ResolveTenantInvitationDto(
    string InvitationPublicId,
    string TenantName,
    string Email,
    string Role,
    string Status,
    DateTime ExpiredAt
);