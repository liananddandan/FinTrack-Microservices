using IdentityService.Common.DTOs;
using IdentityService.Common.Results;
using IdentityService.Common.Status;
using IdentityService.Domain.Entities;
using IdentityService.Events;
using IdentityService.Services.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Common.Results;

namespace IdentityService.Services;

public class UserAppService(UserManager<ApplicationUser> userManager, 
    IUserVerificationService userVerificationService,
    IUserDomainService userDomainService,
    IMediator mediator,
    IJwtTokenService jwtTokenService) : IUserAppService
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
        
        var roleResult = await userDomainService.GetRoleInnerAsync(user, cancellationToken);
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
}