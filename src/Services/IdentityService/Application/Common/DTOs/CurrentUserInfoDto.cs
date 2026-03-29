namespace IdentityService.Application.Common.DTOs;

public record CurrentUserInfoDto(
    string UserPublicId,
    string Email,
    string? UserName,
    IEnumerable<LoginMembershipDto> Memberships
);