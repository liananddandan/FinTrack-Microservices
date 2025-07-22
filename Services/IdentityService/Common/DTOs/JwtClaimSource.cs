namespace IdentityService.Common.DTOs;

public class JwtClaimSource
{
    public required string UserPublicId { get; set; }
    public required string JwtVersion { get; set; }
    public required string TenantPublicId { get; set; }
    public required string UserRoleInTenant { get; set; }
    
}