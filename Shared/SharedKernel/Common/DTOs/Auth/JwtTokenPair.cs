namespace SharedKernel.Common.DTOs.Auth;

public class JwtTokenPair
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}