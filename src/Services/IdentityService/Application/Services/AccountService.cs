using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Services.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using SharedKernel.Common.DTOs.Auth;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Services;

public class AccountService(
    ILogger<AccountService> logger,
    IApplicationUserRepo applicationUserRepo,
    UserManager<ApplicationUser> userManager,
    IJwtTokenService jwtTokenService,
    IUserDomainService userDomainService)
    : IAccountService
{
    public async Task<ServiceResult<UserLoginResult>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        email = email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email))
        {
            return ServiceResult<UserLoginResult>.Fail(
                ResultCodes.Account.LoginParameterError,
                "Email is required.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return ServiceResult<UserLoginResult>.Fail(
                ResultCodes.Account.LoginParameterError,
                "Password is required.");
        }

        try
        {
            var user = await applicationUserRepo.GetUserByEmailWithMembershipsAsync(
                email,
                cancellationToken);

            if (user is null)
            {
                return ServiceResult<UserLoginResult>.Fail(
                    ResultCodes.Account.LoginInvalidCredential,
                    "Invalid email or password.");
            }

            var passwordValid = await userManager.CheckPasswordAsync(user, password);

            if (!passwordValid)
            {
                return ServiceResult<UserLoginResult>.Fail(
                    ResultCodes.Account.LoginInvalidCredential,
                    "Invalid email or password.");
            }

            var activeMemberships = user.Memberships
                .Where(m => m.IsActive && !m.Tenant.IsDeleted)
                .ToList();

            if (!activeMemberships.Any())
            {
                return ServiceResult<UserLoginResult>.Fail(
                    ResultCodes.Account.LoginNoTenant,
                    "User does not belong to any tenant.");
            }

            if (activeMemberships.Count > 1)
            {
                return ServiceResult<UserLoginResult>.Fail(
                    ResultCodes.Account.LoginMultipleTenantsNotSupported,
                    "Multiple tenant memberships are not supported in V1.");
            }

            var membership = activeMemberships.Single();

            var memberships = activeMemberships
                .Select(m => new LoginMembershipDto(
                    m.Tenant.PublicId.ToString(),
                    m.Tenant.Name,
                    m.Role.ToString()))
                .ToList();

            await userDomainService.SyncJwtVersionAsync(user, cancellationToken);

            var accessToken = jwtTokenService.GenerateAccessToken(user, membership);
            var refreshToken = jwtTokenService.GenerateRefreshToken(user, membership);
            var tokens = new JwtTokenPair
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
            return ServiceResult<UserLoginResult>.Ok(
                new UserLoginResult(
                    tokens,
                    memberships),
                ResultCodes.Account.LoginSuccess,
                "Login successful.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Login failed for {Email}", email);

            return ServiceResult<UserLoginResult>.Fail(
                ResultCodes.Account.LoginException,
                "Login failed.");
        }
    }

    public async Task<ServiceResult<JwtTokenPair>> RefreshTokenAsync(
        string userPublicId,
        string tenantPublicId,
        string jwtVersion,
        string userRoleInTenant,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await applicationUserRepo.GetUserByPublicIdWithMembershipsAsync(
                userPublicId,
                cancellationToken);

            if (user is null)
            {
                return ServiceResult<JwtTokenPair>.Fail(
                    ResultCodes.Token.RefreshJwtTokenFailedClaimUserNotFound,
                    "User not found.");
            }

            if (!long.TryParse(jwtVersion, out var parsedJwtVersion) ||
                parsedJwtVersion != user.JwtVersion)
            {
                return ServiceResult<JwtTokenPair>.Fail(
                    ResultCodes.Token.RefreshJwtTokenFailedTokenInvalid,
                    "Invalid jwt version.");
            }

            var membership = user.Memberships
                .FirstOrDefault(m =>
                    m.IsActive &&
                    !m.Tenant.IsDeleted &&
                    m.Tenant.PublicId.ToString() == tenantPublicId);

            if (membership == null)
            {
                return ServiceResult<JwtTokenPair>.Fail(
                    ResultCodes.Token.RefreshJwtTokenFailedClaimTenantIdInvalid,
                    "Tenant membership not found.");
            }

            // refresh 时不增加 jwtVersion，只同步 Redis
            await userDomainService.SyncJwtVersionAsync(user, cancellationToken);

            var accessToken = jwtTokenService.GenerateAccessToken(user, membership);
            var refreshToken = jwtTokenService.GenerateRefreshToken(user, membership);

            return ServiceResult<JwtTokenPair>.Ok(
                new JwtTokenPair
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                },
                ResultCodes.Token.RefreshJwtTokenSuccess,
                "Refresh token success");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to refresh token for user {UserPublicId}",
                userPublicId);

            return ServiceResult<JwtTokenPair>.Fail(
                ResultCodes.Token.RefreshJwtTokenFailedTokenInvalid,
                "Refresh token failed.");
        }
    }
}