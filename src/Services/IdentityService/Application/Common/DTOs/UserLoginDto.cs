using SharedKernel.Common.DTOs.Auth;

namespace IdentityService.Application.Common.DTOs;

public record UserLoginDto(
    JwtTokenPair Tokens,
    IEnumerable<LoginMembershipDto> Memberships
);