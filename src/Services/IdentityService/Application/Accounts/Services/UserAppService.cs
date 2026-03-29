using IdentityService.Application.Accounts.Abstractions;
using IdentityService.Application.Common.DTOs;
using IdentityService.Domain.Entities;
using SharedKernel.Common.DTOs.Auth;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Accounts.Services;

public class UserAppService(
    IApplicationUserRepo applicationUserRepo,
    ILogger<UserAppService> logger) : IUserAppService
{
    public Task<ServiceResult<ApplicationUser>> GetUserByIdAsync(string id)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<ConfirmAccountEmailDto>> ConfirmAccountEmailAsync(string userPublicId, string token, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<UserLoginDto>> UserLoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<bool>> SetUserPasswordAsync(string userPublicId, string oldPassword, string newPassword, bool reset,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<JwtTokenPair>> RefreshUserTokenPairAsync(string userPublicId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<CurrentUserInfoDto>> GetUserInfoAsync(
        string userPublicId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await applicationUserRepo.GetUserByPublicIdWithMembershipsAsync(
                userPublicId,
                cancellationToken);

            if (user is null)
            {
                return ServiceResult<CurrentUserInfoDto>.Fail(
                    ResultCodes.Account.UserNotFound,
                    "User not found.");
            }

            var memberships = user.Memberships
                .Where(m => m.IsActive && !m.Tenant.IsDeleted)
                .Select(m => new LoginMembershipDto(
                    m.Tenant.PublicId.ToString(),
                    m.Tenant.Name,
                    m.Role.ToString()))
                .ToList();

            var result = new CurrentUserInfoDto(
                user.PublicId.ToString(),
                user.Email ?? string.Empty,
                user.UserName,
                memberships);

            return ServiceResult<CurrentUserInfoDto>.Ok(
                result,
                ResultCodes.Account.GetUserInfoSuccess,
                "User info fetched successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get current user info for {UserPublicId}", userPublicId);

            return ServiceResult<CurrentUserInfoDto>.Fail(
                ResultCodes.Account.GetUserInfoException,
                "Failed to get user info.");
        }
    }
}