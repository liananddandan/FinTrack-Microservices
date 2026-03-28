namespace IdentityService.Application.Common.DTOs;

public record TenantInvitationDto(
    string InvitationPublicId,
    string Email,
    string Role,
    string Status,
    DateTime CreatedAt,
    DateTime? AcceptedAt,
    DateTime ExpiredAt,
    string CreatedByUserEmail
);