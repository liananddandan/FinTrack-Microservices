namespace SharedKernel.Contracts.Dev;

public record DevIdentitySeedResult(
    string TenantPublicId,
    string TenantName,
    string AdminUserPublicId,
    string AdminEmail,
    string AdminPassword,
    string MemberUserPublicId,
    string MemberEmail,
    string MemberPassword);