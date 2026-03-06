using SharedKernel.Common.DTOs.Auth;

namespace IdentityService.Common.DTOs;

public class UserLoginResult
{
    public JwtTokenPair? TokenPair { get; set; }
    public required string UserPublicId { get; set; }
    public bool IsFirstLogin { get; set; }
}