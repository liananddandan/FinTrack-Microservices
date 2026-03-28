using System.Security.Claims;
using IdentityService.Application.Common.DTOs;
using IdentityService.Domain.Entities;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Abstractions;

public interface IJwtTokenService
{
    string GenerateAccountAccessToken(ApplicationUser user);

    string GenerateTenantAccessToken(
        ApplicationUser user,
        TenantMembership membership);

    string GenerateRefreshToken(ApplicationUser user);

    string GenerateInvitationToken(TenantInvitation invitation);

    Task<ServiceResult<ClaimsPrincipal?>> GetPrincipalFromTokenAsync(string token);

    Task<ServiceResult<JwtParseResult>> ParseJwtTokenAsync(string token);

    Task<ServiceResult<InvitationParseResult>> ParseInvitationTokenAsync(string token);
}