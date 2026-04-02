namespace IdentityService.Application.Platforms.Dtos;

public class PlatformTokenDto
{
    public required string PlatformAccessToken { get; set; }
    public required string PlatformRole { get; set; }
}