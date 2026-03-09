namespace GatewayService.DTOs;

public class DevTransactionSeedRequest
{
    public string TenantPublicId { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public string AdminUserPublicId { get; set; } = string.Empty;
    public string MemberUserPublicId { get; set; } = string.Empty;
}
