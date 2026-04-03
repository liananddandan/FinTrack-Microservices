namespace IdentityService.Api.Tenants.Contracts;

public sealed class TenantContextResponse
{
    public Guid TenantId { get; set; }

    public string TenantPublicId { get; set; } = default!;

    public string TenantName { get; set; } = default!;

    public string? Host { get; set; }

    public bool IsActive { get; set; }
}