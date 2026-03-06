using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Services.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
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

            var memberships = user.Memberships
                .Where(m => m.IsActive)
                .Select(m => new LoginMembershipDto(
                    m.Tenant.PublicId.ToString(),
                    m.Tenant.Name,
                    m.Role.ToString()))
                .ToList();

            if (!memberships.Any())
            {
                return ServiceResult<UserLoginResult>.Fail(
                    ResultCodes.Account.LoginNoTenant,
                    "User does not belong to any tenant.");
            }
            
            await userDomainService.SyncJwtVersionAsync(user, cancellationToken);

            var accessToken = jwtTokenService.GenerateAccessToken(user, user.Memberships);
            var refreshToken = jwtTokenService.GenerateRefreshToken(user);

            return ServiceResult<UserLoginResult>.Ok(
                new UserLoginResult(
                    accessToken,
                    refreshToken,
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
}