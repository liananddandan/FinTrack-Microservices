namespace IdentityService.Application.Common.DTOs;

public record LoginMembershipDto(
    string TenantPublicId,
    string TenantName,
    string Role
);