using System.Security.Cryptography;
using IdentityService.Common.DTOs;
using IdentityService.Common.Results;
using IdentityService.Common.Status;
using IdentityService.Domain.Entities;
using IdentityService.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Common.Exceptions;
using SharedKernel.Common.Results;

namespace IdentityService.Services;

public class UserService(UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IUserVerificationService userVerificationService) : IUserDomainService, IUserAppService
{
    public async Task<(ApplicationUser, string)> CreateUserOrThrowInnerAsync(string userName, string userEmail,
        long tenantId, CancellationToken cancellationToken = default)
    {
        var randomPassword = GenerateSecurePassword();
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = userEmail,
            EmailConfirmed = false,
            TenantId = tenantId,
        };
        var result = await userManager.CreateAsync(user, randomPassword);
        if (!result.Succeeded)
        {
            var error = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new UserCreateException($"CreateUserAsync failed: {error}");
        }

        return (user, randomPassword);
    }

    public async Task<RoleStatus> CreateRoleInnerAsync(string roleName, CancellationToken cancellationToken = default)
    {
        if (await roleManager.RoleExistsAsync(roleName))
        {
            return RoleStatus.RoleAlreadyExist;
        }
        
        var result = await roleManager.CreateAsync(new ApplicationRole() { Name = roleName });
        return result.Succeeded ? RoleStatus.CreateSuccess : RoleStatus.CreateFailed;
    }

    public async Task<RoleStatus> AddUserToRoleInnerAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken = default)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            return RoleStatus.RoleNotExist;
        }
        var result = await userManager.AddToRoleAsync(user, roleName);
        return result.Succeeded ? RoleStatus.AddRoleToUserSuccess : RoleStatus.AddRoleToUserFailed;
    }

    private string GenerateSecurePassword()
    {
        const int length = 12;
        const string lower = "abcdefghijklmnopqrstuvwxyz";
        const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digits = "1234567890";
        const string specials = "!@#$%^&*()";

        var all = lower + upper + digits + specials;
        var randomChars = new List<char>
        {
            lower[RandomNumberGenerator.GetInt32(lower.Length)],
            upper[RandomNumberGenerator.GetInt32(upper.Length)],
            digits[RandomNumberGenerator.GetInt32(digits.Length)],
            specials[RandomNumberGenerator.GetInt32(specials.Length)]
        };

        while (randomChars.Count < length)
        {
            randomChars.Add(all[RandomNumberGenerator.GetInt32(all.Length)]);
        }

        // Shuttle
        return new string(randomChars.OrderBy(_ => RandomNumberGenerator.GetInt32(100)).ToArray());
    }


    public async Task<ServiceResult<ApplicationUser>> GetUserByIdAsync(string id)
    {
        var result = await userManager.FindByIdAsync(id);
        return result == null 
            ? ServiceResult<ApplicationUser>.Fail(ResultCodes.User.UserNotFound, "User Not Found")
            : ServiceResult<ApplicationUser>.Ok(result, ResultCodes.User.UserGetByIdSuccess, "User Found By Id");
    }

    public async Task<ServiceResult<ConfirmAccountEmailResult>> ConfirmAccountEmailAsync(string userPublicId, string token, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(userPublicId, out var publicUserId))
        {
            return ServiceResult<ConfirmAccountEmailResult>.Fail(ResultCodes.User.UserInvalidPublicid, "Invalid PublicId");
        }

        var user = await userManager.Users.FirstOrDefaultAsync(u => u.PublicId == publicUserId, cancellationToken);

        if (user == null)
        {
            return ServiceResult<ConfirmAccountEmailResult>.Fail(ResultCodes.User.UserNotFound, "User Not Found");
        }
        
        var verificationResult = await userVerificationService.ValidateTokenAsync(user, token, TokenPurpose.EmailConfirmation, cancellationToken);
        if (verificationResult.Success)
        {
            var result = new ConfirmAccountEmailResult(user.PublicId, true);
            return ServiceResult<ConfirmAccountEmailResult>.Ok(result, ResultCodes.User.UserEmailVerificationSuccess, "User Email Verification Success");
        }
        else
        {
            return ServiceResult<ConfirmAccountEmailResult>.Fail(ResultCodes.User.UserEmailVerificationFailed, "User Email Verification Failed");
        }
    }
}