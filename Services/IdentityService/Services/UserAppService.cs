using IdentityService.Common.DTOs;
using IdentityService.Common.Results;
using IdentityService.Common.Status;
using IdentityService.Domain.Entities;
using IdentityService.Events;
using IdentityService.Repositories.Interfaces;
using IdentityService.Services.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Common.DTOs.Auth;
using SharedKernel.Common.Results;

namespace IdentityService.Services;

public class UserAppService(UserManager<ApplicationUser> userManager, 
    IUserVerificationService userVerificationService,
    IUserDomainService userDomainService,
    IMediator mediator,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork) : IUserAppService
{
        public async Task<ServiceResult<ApplicationUser>> GetUserByIdAsync(string id)
    {
        var result = await userManager.FindByIdAsync(id);
        return result == null
            ? ServiceResult<ApplicationUser>
                .Fail(ResultCodes.User.UserNotFound, "User Not Found")
            : ServiceResult<ApplicationUser>
                .Ok(result, ResultCodes.User.UserGetByIdSuccess, "User Found By Id");
    }

    public async Task<ServiceResult<ConfirmAccountEmailResult>> ConfirmAccountEmailAsync(string userPublicId,
        string token, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(userPublicId, out var publicUserId))
        {
            return ServiceResult<ConfirmAccountEmailResult>
                .Fail(ResultCodes.User.UserPublicIdInvalid,"Invalid PublicId");
        }

        var user = await userManager.Users.FirstOrDefaultAsync(u => u.PublicId == publicUserId, cancellationToken);

        if (user == null)
        {
            return ServiceResult<ConfirmAccountEmailResult>
                .Fail(ResultCodes.User.UserNotFound, "User Not Found");
        }

        var verificationResult =
            await userVerificationService.ValidateTokenAsync(user, token, TokenPurpose.EmailConfirmation,
                cancellationToken);
        if (verificationResult.Success)
        {
            var result = new ConfirmAccountEmailResult(user.PublicId, true);
            return ServiceResult<ConfirmAccountEmailResult>
                .Ok(result, ResultCodes.User.UserEmailVerificationSuccess, "User Email Verification Success");
        }
        else
        {
            return ServiceResult<ConfirmAccountEmailResult>
                .Fail(ResultCodes.User.UserEmailVerificationFailed, "User Email Verification Failed");
        }
    }

    public async Task<ServiceResult<UserLoginResult>> UserLoginAsync(string email, string password,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.Users
            .Include(u => u.Tenant)
            .Where(u => u.Email == email)
            .FirstOrDefaultAsync(cancellationToken);
        if (user == null)
        {
            return ServiceResult<UserLoginResult>
                .Fail(ResultCodes.User.UserNotFound, "User Not Found");
        }

        if (!user.EmailConfirmed)
        {
            return ServiceResult<UserLoginResult>
                .Fail(ResultCodes.User.UserEmailNotVerified, "User Email Not Verified");
        }

        var checkPasswordResult = await userManager.CheckPasswordAsync(user, password);
        if (!checkPasswordResult)
        {
            return ServiceResult<UserLoginResult>
                .Fail(ResultCodes.User.UserEmailOrPasswordInvalid, "User Email Or Password Invalid");
        }
        
        var roleResult = await userDomainService.GetUserRoleInnerAsync(user, cancellationToken);
        if (string.IsNullOrEmpty(roleResult))
        {
            return ServiceResult<UserLoginResult>
                .Fail(ResultCodes.User.UserCouldNotFindRole, "User role Not Found");
        }

        var userLoginResult = new UserLoginResult()
        {
            IsFirstLogin = true,
            UserPublicId = user.PublicId.ToString()
        };

        if (user.IsFirstLogin)
        {
            await mediator.Publish(new UserFirstLoginEvent(){
                UserPublicId = user.PublicId.ToString(), 
                JwtVersion = user.JwtVersion.ToString(),
                TenantPublicId = user.Tenant!.PublicId.ToString(),
                UserEmail = user.Email!,
                UserRoleInTenant = roleResult}, 
                cancellationToken);
            return ServiceResult<UserLoginResult>
                .Ok(userLoginResult, ResultCodes.User.UserLoginSuccessButFirstLogin, "User First Login Success.");
        }
        var jwtClaimSource = new JwtClaimSource()
        {
            UserPublicId = user.PublicId.ToString(),
            JwtVersion = user.JwtVersion.ToString(),
            TenantPublicId = user.Tenant!.PublicId.ToString(),
            UserRoleInTenant = roleResult
        };
        var jwtTokenPair = await jwtTokenService.GenerateJwtTokenPairAsync(jwtClaimSource);
        userLoginResult.IsFirstLogin = false;
        userLoginResult.TokenPair = jwtTokenPair.Data;
        return ServiceResult<UserLoginResult>
            .Ok(userLoginResult, ResultCodes.User.UserLoginSuccess, "User Login Success.");
    }

    public async Task<ServiceResult<bool>> SetUserPasswordAsync(string userPublicId, 
        string oldPassword, string newPassword, bool reset, CancellationToken cancellationToken = default)
    {
        var userCheckResult = await GetUserIncludeTenantAsync(userPublicId, cancellationToken);
        if (!userCheckResult.Success)
        {
            return ServiceResult<bool>.Fail(userCheckResult.Code!, userCheckResult.Message!);
        }
        var user = userCheckResult.Data!;
        if (reset && user.IsFirstLogin)
        {
            return ServiceResult<bool>.Fail(ResultCodes.User.UserResetPasswordBeforeSetPasswordFailed, "User Reset Password Before Set Password Failed");
        }

        var (transactionResult, reason) = await unitOfWork.WithTransactionAsync(async () =>
        {
            var (changePasswordResult, reason) = await userDomainService
                .ChangePasswordInnerAsync(user, oldPassword, newPassword, cancellationToken);
            if (!changePasswordResult)
            {
                return (changePasswordResult, reason);
            }

            if (!reset)
            {
                await userDomainService.ChangeFirstLoginStateInnerAsync(user, cancellationToken);
            }

            await userDomainService.IncreaseUserJwtVersionInnerAsync(user, cancellationToken);

            return (changePasswordResult, reason);
        }, cancellationToken);
        return transactionResult
            ? ServiceResult<bool>.Ok(true, ResultCodes.User.UserChangePasswordSuccess, "User Change Password Success")
            : ServiceResult<bool>.Fail(ResultCodes.User.UserSetPasswordFailed, reason);
    }

    public async Task<ServiceResult<JwtTokenPair>> RefreshUserTokenPairAsync(string userPublicId, CancellationToken cancellationToken = default)
    {
        var userCheckResult = await GetUserIncludeTenantAsync(userPublicId, cancellationToken);
        if (!userCheckResult.Success)
        {
            return ServiceResult<JwtTokenPair>.Fail(userCheckResult.Code!, userCheckResult.Message!);
        }

        var user = userCheckResult.Data!;
        var roleResult = await userDomainService.GetUserRoleInnerAsync(user, cancellationToken);
        if (string.IsNullOrEmpty(roleResult))
        {
            return ServiceResult<JwtTokenPair>.Fail(ResultCodes.User.UserCouldNotFindRole, "User role Not Found");
        }

        var jwtClaimSource = new JwtClaimSource()
        {
            UserPublicId = user.PublicId.ToString(),
            JwtVersion = user.JwtVersion.ToString(),
            TenantPublicId = user.Tenant!.PublicId.ToString(),
            UserRoleInTenant = roleResult
        };
       var generateResult =  await jwtTokenService.GenerateJwtTokenPairAsync(jwtClaimSource);
       return generateResult.Success
           ? ServiceResult<JwtTokenPair>.Ok(generateResult.Data!, ResultCodes.User.UserRefreshTokenSuccess,
               "User Refresh Token Success")
           : generateResult;
    }

    public async Task<ServiceResult<UserInfoDto>> GetUserInfoAsync(string userPublicId, CancellationToken cancellationToken = default)
    {
        var userCheckResult = await GetUserIncludeTenantAsync(userPublicId, cancellationToken);
        if (!userCheckResult.Success)
        {
            return ServiceResult<UserInfoDto>.Fail(userCheckResult.Code!, userCheckResult.Message!);
        }
        var user = userCheckResult.Data!;
        if (user.Tenant == null)
        {
            return ServiceResult<UserInfoDto>.Fail(ResultCodes.User.UserTenantInfoMissed, "User Tenant Not Found");
        }

        var roleResult = await userDomainService.GetUserRoleInnerAsync(user, cancellationToken);
        if (roleResult == null)
        {
            return ServiceResult<UserInfoDto>.Fail(ResultCodes.User.UserCouldNotFindRole, "User role Not Found");
        }

        var tenant = user.Tenant;
        var tenantInfo = new TenantInfoDto()
        {
            TenantPublicId = tenant.PublicId.ToString(),
            TenantName = tenant.Name
        };
        var userInfoDto = new UserInfoDto()
        {
            userPublicId = user.PublicId.ToString(),
            userName = user.UserName,
            email = user.Email!,
            roleName = roleResult,
            tenantInfoDto = tenantInfo
        };
        return ServiceResult<UserInfoDto>.Ok(userInfoDto, ResultCodes.User.UserGetInfoSuccess, "User Info Success");
    }

    private async Task<ServiceResult<ApplicationUser>> GetUserIncludeTenantAsync(string userPublicId, CancellationToken cancellationToken = default)
    {
        var user = await userDomainService.GetUserByPublicIdIncludeTenantAsync(userPublicId, cancellationToken);
        if (user == null)
        {
            return ServiceResult<ApplicationUser>.Fail(ResultCodes.User.UserNotFound, "User Not Found");
        }
        
        return ServiceResult<ApplicationUser>
            .Ok(user, ResultCodes.User.UserCheckPublicIdAndJwtVersionSuccess, "User Check Public Id and JWT Version Success");
    }
}