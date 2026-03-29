namespace GatewayService.Application.Dev.Dtos;

public record DevSeedTenantResult(
    string TenantPublicId,
    string TenantName,
    string AdminEmail,
    string AdminPassword,
    string MemberEmail,
    string MemberPassword,
    int CategoryCount,
    int ProductCount,
    int OrderCount);

public record DevSeedResult(
    IReadOnlyList<DevSeedTenantResult> Tenants);

