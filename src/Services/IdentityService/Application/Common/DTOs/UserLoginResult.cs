using SharedKernel.Common.DTOs.Auth;

namespace IdentityService.Application.Common.DTOs;

public record UserLoginResult(
    JwtTokenPair Tokens,
    IEnumerable<LoginMembershipDto> Memberships
);