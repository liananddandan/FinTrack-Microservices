using SharedKernel.Common.DTOs.Auth;

namespace IdentityService.Application.Common.DTOs;

public record UserLoginResult(
    string AccessToken,
    string RefreshToken,
    IEnumerable<LoginMembershipDto> Memberships
);