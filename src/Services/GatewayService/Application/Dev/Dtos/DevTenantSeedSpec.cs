namespace GatewayService.Application.Dev.Dtos;
public record DevTenantSeedSpec(
    string TenantName,
    string AdminEmail,
    string AdminPassword,
    string MemberEmail,
    string MemberPassword,
    string Template);
