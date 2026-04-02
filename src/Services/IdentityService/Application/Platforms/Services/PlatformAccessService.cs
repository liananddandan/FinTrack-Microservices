using IdentityService.Application.Accounts.Abstractions;
using IdentityService.Application.Common.Abstractions;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Platforms.Abstractions;
using IdentityService.Application.Platforms.Dtos;
using SharedKernel.Common.Constants;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Platforms.Services;

public class PlatformAccessService(
    IApplicationUserRepo userRepository,
    IPlatformAccessRepository platformAccessRepository,
    IJwtTokenService jwtTokenService)
    : IPlatformAccessService
{
    public async Task<ServiceResult<PlatformTokenDto>> SelectPlatformAsync(
        string userPublicId,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetUserByPublicIdAsync(userPublicId, cancellationToken);
        if (user is null)
        {
            return ServiceResult<PlatformTokenDto>.Fail(
                "Identity.User.NotFound",
                "User not found.");
        }

        var platformAccess = await platformAccessRepository.GetByUserPublicIdAsync(
            userPublicId,
            cancellationToken);

        if (platformAccess is null || !platformAccess.IsEnabled)
        {
            return ServiceResult<PlatformTokenDto>.Fail(
                "Identity.PlatformAccess.Forbidden",
                "User does not have platform access.");
        }

        var claimSource = new JwtClaimSource
        {
            UserPublicId = user.PublicId.ToString(),
            JwtVersion = user.JwtVersion.ToString(),
            TenantPublicId = String.Empty,
            UserRoleInTenant = String.Empty,
            HasPlatformAccess = true,
            PlatformRole = platformAccess.Role
        };

        var token = jwtTokenService.GeneratePlatformAccessToken(claimSource);

        return ServiceResult<PlatformTokenDto>.Ok(
            token,
            "Identity.PlatformAccess.SelectSuccess",
            "Platform access token generated successfully.");
    }

    public async Task<ServiceResult<bool>> HasPlatformAccessAsync(
        string userPublicId,
        CancellationToken cancellationToken = default)
    {
        var exists = await platformAccessRepository.ExistsEnabledAccessAsync(
            userPublicId,
            cancellationToken);

        return ServiceResult<bool>.Ok(
            exists,
            "Identity.PlatformAccess.CheckSuccess",
            "Platform access checked successfully.");
    }
}