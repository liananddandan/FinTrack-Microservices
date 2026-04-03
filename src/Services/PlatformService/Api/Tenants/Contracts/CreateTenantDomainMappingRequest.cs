namespace PlatformService.Api.Tenants.Contracts;

public class CreateTenantDomainMappingRequest
{
    public Guid TenantPublicId { get; set; }

    public string Host { get; set; } = string.Empty;

    public string DomainType { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public bool IsActive { get; set; } = true;
}