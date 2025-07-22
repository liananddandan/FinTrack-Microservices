namespace IdentityService.Common.DTOs;

public class UserInfoDto
{
    public string email { get; set; }
    public string? userName { get; set; }
    public string userPublicId { get; set; }
    public TenantInfoDto tenantInfoDto { get; set; }
    public string roleName { get; set; }
}