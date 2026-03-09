namespace GatewayService.DTOs;

public record DevSeedResult(
    string TenantPublicId,
    string TenantName,
    string AdminEmail,
    string AdminPassword,
    string MemberEmail,
    string MemberPassword,
    int DonationCount,
    int ProcurementCount
);
