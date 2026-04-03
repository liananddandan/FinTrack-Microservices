namespace PlatformService.Api.Tenants.Contracts;

public class UpdateTenantDomainMappingRequest
{
    public string Host { get; set; } = string.Empty;

    public string DomainType { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public bool IsActive { get; set; }
}