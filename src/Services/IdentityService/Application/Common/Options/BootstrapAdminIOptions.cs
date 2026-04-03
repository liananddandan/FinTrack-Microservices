namespace IdentityService.Application.Common.Options;

public class BootstrapAdminOptions
{
    public bool Enabled { get; set; } = true;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PlatformRole { get; set; } = "SuperAdmin";
}