namespace IdentityService.Application.Common.DTOs;

public record RegisterTenantDto(
    string TenantPublicId,
    string AdminUserPublicId,
    string AdminEmail
);