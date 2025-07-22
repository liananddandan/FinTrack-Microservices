using System.Web;
using IdentityService.Common.Results;
using IdentityService.Common.Status;
using IdentityService.Domain.Entities;
using IdentityService.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using SharedKernel.Common.Results;

namespace IdentityService.Services;

public class UserVerificationService(UserManager<ApplicationUser> userManager) : IUserVerificationService
{
    public async Task<ServiceResult<string>> GenerateTokenAsync(ApplicationUser user,
        TokenPurpose purpose, CancellationToken cancellationToken = default)
    {
        var result = string.Empty;
        switch (purpose)
        {
            case TokenPurpose.EmailConfirmation:
                result = await userManager.GenerateEmailConfirmationTokenAsync(user);
                break;
        }
        if (string.IsNullOrEmpty(result))
        {
            return ServiceResult<string>.Fail(ResultCodes.Token.VerifyTokenGenerateFailed, 
                "Verify token generation failed.");
        }
        return ServiceResult<string>.Ok(result, ResultCodes.Token.VerifyTokenGenerateSuccess, 
            "Verify token generation success.");
    }

    public async Task<ServiceResult<bool>> ValidateTokenAsync(ApplicationUser user, 
        string token, TokenPurpose purpose,
        CancellationToken cancellationToken = default)
    {
        var result = false;
        var message = "";
        switch (purpose)
        {
            case TokenPurpose.EmailConfirmation:
                var emailVerifyResult = await userManager.ConfirmEmailAsync(user, token);
                result = emailVerifyResult.Succeeded;
                message = emailVerifyResult.Errors.Select(e => e.Description).FirstOrDefault();
                break;
            default:
                return ServiceResult<bool>.Fail(ResultCodes.Token.VerifyTokenGenerateInvalidTokenType, "Token Type is not supported.");
        }

        return result 
            ? ServiceResult<bool>.Ok(result, ResultCodes.Token.VerifyTokenSuccess, "Token Verification Successful.")
            : ServiceResult<bool>.Fail(ResultCodes.Token.VerifyTokenFailed, string.IsNullOrEmpty(message) ? "Token Verification failed." : message);
    }
}