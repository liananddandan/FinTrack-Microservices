using System.Security.Cryptography;
using IdentityService.Application.Common.Status;
using IdentityService.Application.Services.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Common.Constants;
using StackExchange.Redis;

namespace IdentityService.Application.Services;

public class UserDomainService(
    UserManager<ApplicationUser> userManager,
    ILogger<UserDomainService> logger,
    IApplicationUserRepo userRepo,
    IConnectionMultiplexer redis) : IUserDomainService
{
    public Task<(ApplicationUser, string)> CreateUserOrThrowInnerAsync(string userName, string userEmail, long tenantId, long roleId,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<RoleStatus> IsRoleExistAsync(string roleName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<RoleStatus> CreateRoleInnerAsync(string roleName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<RoleStatus> AddUserToRoleInnerAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<string?> GetUserRoleInnerAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<(bool, string)> ChangePasswordInnerAsync(ApplicationUser user, string oldPassword, string newPassword,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ApplicationUser?> GetUserByEmailInnerAsync(string userEmail, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ApplicationUser?> GetUserByEmailWithTenantInnerAsync(string userEmail, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task ChangeFirstLoginStateInnerAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task IncreaseUserJwtVersionInnerAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ApplicationUser?> GetUserByPublicIdIncludeTenantAndRoleAsync(string userPublicId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    
    public Task<IEnumerable<ApplicationUser>> GetAllUsersInTenantIncludeRoleAsync(long tenantId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task SyncJwtVersionAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var key = $"{Constant.Redis.JwtVersionPrefix}{user.PublicId}";

        await redis.GetDatabase().StringSetAsync(
            key,
            user.JwtVersion.ToString(),
            TimeSpan.FromDays(30));
    }

    public async Task<ApplicationUser?> GetUserByPublicIdAsync(
        string userPublicId,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(userPublicId, out var parsedUserPublicId))
        {
            return null;
        }

        return await userManager.Users
            .FirstOrDefaultAsync(u => u.PublicId == parsedUserPublicId, cancellationToken);
    }
}