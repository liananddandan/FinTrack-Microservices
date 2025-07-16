using System.Security.Cryptography;
using IdentityService.Common.Status;
using IdentityService.Domain.Entities;
using IdentityService.Repositories.Interfaces;
using IdentityService.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Common.Exceptions;

namespace IdentityService.Services;

public class UserDomainService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    ILogger<UserDomainService> logger,
    IApplicationUserRepo userRepo) : IUserDomainService
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
        if (await roleManager.RoleExistsAsync(roleName.ToUpperInvariant()))
        {
            return RoleStatus.RoleAlreadyExist;
        }

        var result = await roleManager.CreateAsync(new ApplicationRole() { Name = roleName });
        return result.Succeeded ? RoleStatus.CreateSuccess : RoleStatus.CreateFailed;
    }

    public async Task<RoleStatus> AddUserToRoleInnerAsync(ApplicationUser user, string roleName,
        CancellationToken cancellationToken = default)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            return RoleStatus.RoleNotExist;
        }

        var result = await userManager.AddToRoleAsync(user, roleName);
        return result.Succeeded ? RoleStatus.AddRoleToUserSuccess : RoleStatus.AddRoleToUserFailed;
    }

    public async Task<ApplicationUser?> GetUserByPublicIdIncludeTenantAsync(string userPublicId,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(userPublicId, out var uPublicId))
        {
            return null;
        }

        var user = await userManager.Users.Include(u => u.Tenant)
            .Where(u => u.PublicId == uPublicId)
            .FirstOrDefaultAsync(cancellationToken);
        return user;
    }

    public async Task<string?> GetRoleInnerAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var roles = await userManager.GetRolesAsync(user);
        return roles.FirstOrDefault();
    }

    public async Task<bool> ChangePasswordInnerAsync(ApplicationUser user, string oldPassword, string newPassword,
        CancellationToken cancellationToken = default)
    {
        var changePasswordResult = await userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        if (!changePasswordResult.Succeeded)
        {
            logger.LogError($"ChangePasswordInner failed: {string.Join(", ", changePasswordResult.Errors)}");
        }
        return changePasswordResult.Succeeded;
    }

    public async Task<ApplicationUser?> GetUserByEmailInnerAsync(string userEmail, CancellationToken cancellationToken = default)
    {
        return await userManager.FindByEmailAsync(userEmail);
    }

    public Task<ApplicationUser?> GetUserByEmailWithTenantInnerAsync(string userEmail, CancellationToken cancellationToken = default)
    {
        return userRepo.GetUserByEmailWithTenant(userEmail, cancellationToken);
    }

    public Task ChangeFirstLoginStateInnerAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        userRepo.ChangeFirstLoginStatus(user, cancellationToken);
        return Task.CompletedTask;
    }

    public Task IncreaseUserJwtVersionInnerAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        userRepo.IncreaseJwtVersion(user, cancellationToken);
        return Task.CompletedTask;
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
}