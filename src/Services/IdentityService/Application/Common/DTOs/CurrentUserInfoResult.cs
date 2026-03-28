namespace IdentityService.Application.Common.DTOs;

public record CurrentUserInfoResult(
    string UserPublicId,
    string Email,
    string? UserName,
    IEnumerable<LoginMembershipDto> Memberships
);