namespace IdentityService.Application.Common.DTOs;

public record CurrentUserInfoDto(
    string UserPublicId,
    string Email,
    bool EmailConfirmed,
    string? UserName,
    IEnumerable<LoginMembershipDto> Memberships
);