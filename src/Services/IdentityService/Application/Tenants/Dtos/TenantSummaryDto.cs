namespace IdentityService.Application.Tenants.Dtos;

public class TenantSummaryDto
{
    public required string TenantPublicId { get; set; }
    public required string TenantName { get; set; }
    public required bool IsActive { get; set; }
    public required DateTime CreatedAt { get; set; }
}