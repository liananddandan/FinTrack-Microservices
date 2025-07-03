using System.Security.Cryptography;
using IdentityService.Common.Results;
using IdentityService.Common.Status;
using IdentityService.Domain.Entities;
using IdentityService.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using SharedKernel.Common.Exceptions;

namespace IdentityService.Services;

public class UserService(UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager) : IUserDomainService, IUserAppService
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
        const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";
        return new string(Enumerable.Range(0, length)
            .Select(_ => valid[RandomNumberGenerator.GetInt32(valid.Length)]).ToArray());
    }

    public async Task<ServiceResult<ApplicationUser>> GetUserByIdAsync(string id)
    {
        var result = await userManager.FindByIdAsync(id);
        return result == null 
            ? ServiceResult<ApplicationUser>.Fail(ResultCodes.User.UserNotFound, "User Not Found")
            : ServiceResult<ApplicationUser>.Ok(result, ResultCodes.User.UserGetByIdSuccess, "User Found By Id");
    }
}