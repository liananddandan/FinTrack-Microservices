using System.Security.Claims;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Platforms.Dtos;
using IdentityService.Domain.Entities;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Common.Abstractions;

public interface IJwtTokenService
{
    string GenerateAccountAccessToken(ApplicationUser user);

    string GenerateTenantAccessToken(
        ApplicationUser user,
        TenantMembership membership);

    string GenerateRefreshToken(ApplicationUser user);

    string GenerateInvitationToken(TenantInvitation invitation);

    Task<ServiceResult<ClaimsPrincipal?>> GetPrincipalFromTokenAsync(string token);

    Task<ServiceResult<JwtParseDto>> ParseJwtTokenAsync(string token);

    Task<ServiceResult<InvitationParseDto>> ParseInvitationTokenAsync(string token);

    PlatformTokenDto GeneratePlatformAccessToken(JwtClaimSource claimSource);

}