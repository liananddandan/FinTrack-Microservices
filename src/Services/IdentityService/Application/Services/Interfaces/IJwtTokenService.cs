using System.Security.Claims;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Common.Status;
using IdentityService.Domain.Entities;
using SharedKernel.Common.DTOs.Auth;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Services.Interfaces;

public interface IJwtTokenService
{
    Task<ServiceResult<string>> GenerateJwtTokenAsync(JwtClaimSource jwtClaimSource, JwtTokenType type);
    Task<ServiceResult<JwtTokenPair>> GenerateJwtTokenPairAsync(JwtClaimSource jwtClaimSource);
    Task<ServiceResult<JwtTokenPair>> RefreshJwtTokenPairAsync(string oldRefreshToken);
    Task<ServiceResult<ClaimsPrincipal?>> GetPrincipalFromTokenAsync(string token);
    Task<ServiceResult<string>> GenerateInvitationTokenAsync(InvitationClaimSource invitationClaimSource);
    Task<ServiceResult<InvitationParseResult>> ParseInvitationTokenAsync(string token);
    
    // updated
    string GenerateAccessToken(ApplicationUser user, IEnumerable<TenantMembership> memberships);
    string GenerateRefreshToken(ApplicationUser user);
    string GenerateInvitationToken(TenantInvitation invitation);
    Task<ServiceResult<JwtParseResult>> ParseJwtTokenAsync(string token);

}