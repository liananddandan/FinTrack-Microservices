using IdentityService.Common.Results;
using IdentityService.Common.Status;
using IdentityService.Domain.Entities;
using IdentityService.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Services;

public class UserVerificationService(UserManager<ApplicationUser> userManager) : IUserVerificationService
{
    public async Task<ServiceResult<string>> GenerateTokenAsync(ApplicationUser user,
        TokenPurpose purpose, CancellationToken cancellationToken = default)
    {
        var result =
            await userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultProvider, 
                purpose.ToIdentityString());
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
        var result = await userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultProvider, token, purpose.ToIdentityString());
        return ServiceResult<bool>.Ok(result, 
            ResultCodes.Token.VerifyTokenProcessFinished, 
            "Verify token process finished.");
    }
}