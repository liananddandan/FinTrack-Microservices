namespace IdentityService.Application.Common.DTOs;

public record TenantMemberDto(
    string UserPublicId,
    string Email,
    string UserName,
    string Role,
    bool IsActive,
    DateTime JoinedAt
);