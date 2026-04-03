namespace IdentityService.Application.Tenants.Dtos;


public class TenantContextDto
{
    public string TenantPublicId { get; set; } = string.Empty;

    public string TenantName { get; set; } = string.Empty;

    public string Host { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public string? LogoUrl { get; set; }

    public string? ThemeColor { get; set; }
}