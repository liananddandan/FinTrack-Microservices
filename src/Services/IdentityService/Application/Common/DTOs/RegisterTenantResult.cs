namespace IdentityService.Application.Common.DTOs;

public record RegisterTenantResult(
    Guid PublicTenantId,
    string AdminEmail,
    string TemporaryPassword
);