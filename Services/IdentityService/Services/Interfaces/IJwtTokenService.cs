using System.Security.Claims;
using IdentityService.Common.DTOs;
using IdentityService.Common.Status;
using SharedKernel.Common.DTOs.Auth;
using SharedKernel.Common.Results;

namespace IdentityService.Services.Interfaces;

public interface IJwtTokenService
{
    Task<ServiceResult<string>> GenerateJwtTokenAsync(JwtClaimSource jwtClaimSource, JwtTokenType type);
    Task<ServiceResult<JwtTokenPair>> GenerateJwtTokenPairAsync(JwtClaimSource jwtClaimSource);
    Task<ServiceResult<JwtTokenPair>> RefreshJwtTokenPairAsync(string oldRefreshToken);
    Task<ServiceResult<ClaimsPrincipal?>> GetPrincipalFromTokenAsync(string token);
    Task<ServiceResult<JwtParseResult>> ParseJwtTokenAsync(string token);
    Task<ServiceResult<string>> GenerateInvitationTokenAsync(InvitationClaimSource invitationClaimSource);
    Task<ServiceResult<InvitationParseResult>> ParseInvitationTokenAsync(string token);
}