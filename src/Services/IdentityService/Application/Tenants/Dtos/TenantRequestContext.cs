namespace IdentityService.Application.Tenants.Dtos;

public class TenantRequestContext
{
    public Guid TenantPublicId { get; set; }

    public string Host { get; set; } = string.Empty;

    public string DomainType { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public bool IsActive { get; set; }
}