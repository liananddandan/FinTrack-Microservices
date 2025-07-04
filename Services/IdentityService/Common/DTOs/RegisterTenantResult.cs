namespace IdentityService.Common.DTOs;

public record RegisterTenantResult(
    Guid PublicTenantId,
    string AdminEmail,
    string TemporaryPassword
);