using IdentityService.Application.Accounts.Abstractions;
using IdentityService.Application.Accounts.Events;
using IdentityService.Application.Common.Abstractions;
using IdentityService.Application.Common.DTOs;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SharedKernel.Common.DTOs.Auth;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Accounts.Services;

public class AccountService(
    ILogger<AccountService> logger,
    IApplicationUserRepo applicationUserRepo,
    UserManager<ApplicationUser> userManager,
    IJwtTokenService jwtTokenService,
    IUserDomainService userDomainService,
    IEmailVerificationService emailVerificationService,
    IMediator mediator)
    : IAccountService
{
    public async Task<ServiceResult<UserLoginDto>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        email = email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email))
        {
            return ServiceResult<UserLoginDto>.Fail(
                ResultCodes.Account.LoginParameterError,
                "Email is required.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return ServiceResult<UserLoginDto>.Fail(
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
                return ServiceResult<UserLoginDto>.Fail(
                    ResultCodes.Account.LoginInvalidCredential,
                    "Invalid email or password.");
            }

            var passwordValid = await userManager.CheckPasswordAsync(user, password);

            if (!passwordValid)
            {
                return ServiceResult<UserLoginDto>.Fail(
                    ResultCodes.Account.LoginInvalidCredential,
                    "Invalid email or password.");
            }

            var activeMemberships = user.Memberships
                .Where(m => m.IsActive && !m.Tenant.IsDeleted)
                .ToList();

            var memberships = activeMemberships
                .Select(m => new LoginMembershipDto(
                    m.Tenant.PublicId.ToString(),
                    m.Tenant.Name,
                    m.Role.ToString()))
                .ToList();

            await userDomainService.SyncJwtVersionAsync(user, cancellationToken);

            var accountAccessToken = jwtTokenService.GenerateAccountAccessToken(user);
            var refreshToken = jwtTokenService.GenerateRefreshToken(user);

            var tokens = new JwtTokenPair
            {
                AccessToken = accountAccessToken,
                RefreshToken = refreshToken
            };

            return ServiceResult<UserLoginDto>.Ok(
                new UserLoginDto(
                    tokens,
                    memberships),
                ResultCodes.Account.LoginSuccess,
                "Login successful.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Login failed for {Email}", email);

            return ServiceResult<UserLoginDto>.Fail(
                ResultCodes.Account.LoginException,
                "Login failed.");
        }
    }

    public async Task<ServiceResult<JwtTokenPair>> RefreshTokenAsync(
        string userPublicId,
        string jwtVersion,
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

            await userDomainService.SyncJwtVersionAsync(user, cancellationToken);

            var accountAccessToken = jwtTokenService.GenerateAccountAccessToken(user);
            var refreshToken = jwtTokenService.GenerateRefreshToken(user);

            return ServiceResult<JwtTokenPair>.Ok(
                new JwtTokenPair
                {
                    AccessToken = accountAccessToken,
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

    public async Task<ServiceResult<RegisterUserDto>> RegisterUserAsync(
        string userName,
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        userName = userName.Trim();
        email = email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(userName))
        {
            return ServiceResult<RegisterUserDto>.Fail(
                ResultCodes.Account.RegisterUserParameterError,
                "User name is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return ServiceResult<RegisterUserDto>.Fail(
                ResultCodes.Account.RegisterUserParameterError,
                "Email is required.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return ServiceResult<RegisterUserDto>.Fail(
                ResultCodes.Account.RegisterUserParameterError,
                "Password is required.");
        }

        try
        {
            var emailExists = await applicationUserRepo.IsEmailExistsAsync(email, cancellationToken);
            if (emailExists)
            {
                return ServiceResult<RegisterUserDto>.Fail(
                    ResultCodes.Account.RegisterUserEmailExists,
                    "Email already exists.");
            }

            var user = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                EmailConfirmed = false
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                var error = string.Join(", ", createResult.Errors.Select(e => e.Description));
                return ServiceResult<RegisterUserDto>.Fail(
                    ResultCodes.Account.RegisterUserCreateFailed,
                    error);
            }
            
            var verificationResult = await emailVerificationService.CreateTokenAsync(
                user.Id,
                createdByIp: null,
                cancellationToken: cancellationToken);

            if (!verificationResult.Success || verificationResult.Data is null)
            {
                logger.LogWarning(
                    "User {UserId} registered successfully, but failed to create email verification token.",
                    user.Id);
            }
            else
            {
                await mediator.Publish(
                    new UserRegisteredEvent(
                        user.Id,
                        user.Email!,
                        user.UserName!,
                        verificationResult.Data.RawToken,
                        verificationResult.Data.ExpiresAtUtc),
                    cancellationToken);
            }

            return ServiceResult<RegisterUserDto>.Ok(
                new RegisterUserDto(
                    user.PublicId.ToString(),
                    user.Email!,
                    user.UserName!),
                ResultCodes.Account.RegisterUserSuccess,
                "User registered successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to register user {Email}", email);

            return ServiceResult<RegisterUserDto>.Fail(
                ResultCodes.Account.RegisterUserException,
                "User registration failed.");
        }
    }
    
    public async Task<ServiceResult<string>> SelectTenantAsync(
        string userPublicId,
        string tenantPublicId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userPublicId))
        {
            return ServiceResult<string>.Fail(
                ResultCodes.Account.SelectTenantParameterError,
                "User public id is required.");
        }

        if (string.IsNullOrWhiteSpace(tenantPublicId))
        {
            return ServiceResult<string>.Fail(
                ResultCodes.Account.SelectTenantParameterError,
                "Tenant public id is required.");
        }

        try
        {
            var user = await applicationUserRepo.GetUserByPublicIdWithMembershipsAsync(
                userPublicId,
                cancellationToken);

            if (user is null)
            {
                return ServiceResult<string>.Fail(
                    ResultCodes.Account.SelectTenantUserNotFound,
                    "User not found.");
            }

            var membership = user.Memberships.FirstOrDefault(m =>
                m.IsActive &&
                !m.Tenant.IsDeleted &&
                m.Tenant.PublicId.ToString() == tenantPublicId);

            if (membership is null)
            {
                return ServiceResult<string>.Fail(
                    ResultCodes.Account.SelectTenantMembershipNotFound,
                    "Tenant membership not found.");
            }

            var tenantAccessToken = jwtTokenService.GenerateTenantAccessToken(user, membership);

            return ServiceResult<string>.Ok(
                tenantAccessToken,
                ResultCodes.Account.SelectTenantSuccess,
                "Tenant selected successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to select tenant {TenantPublicId} for user {UserPublicId}",
                tenantPublicId,
                userPublicId);

            return ServiceResult<string>.Fail(
                ResultCodes.Account.SelectTenantException,
                "Failed to select tenant.");
        }
    }
}