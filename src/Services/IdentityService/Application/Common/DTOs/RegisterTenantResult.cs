namespace IdentityService.Application.Common.DTOs;

public record RegisterTenantResult(
    string TenantPublicId,
    string AdminUserPublicId,
    string AdminEmail
);