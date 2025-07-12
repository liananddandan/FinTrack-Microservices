using IdentityService.Common.Status;
using IdentityService.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Services.Interfaces;

public interface IUserDomainService
{
    Task<(ApplicationUser, string)> CreateUserOrThrowInnerAsync(string userName, string userEmail, 
        long tenantId, CancellationToken cancellationToken = default);
    Task<RoleStatus> CreateRoleInnerAsync(string roleName, CancellationToken cancellationToken = default);
    Task<RoleStatus> AddUserToRoleInnerAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetUserByPublicIdIncludeTenantAsync(string userPublicId, CancellationToken cancellationToken = default);
    Task<string?> GetRoleInnerAsync(ApplicationUser user, CancellationToken cancellationToken = default);
    Task<bool> ChangePasswordInnerAsync(ApplicationUser user, string oldPassword, string newPassword, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetUserByEmailInnerAsync(string userEmail, CancellationToken cancellationToken = default);
    Task ChangeFirstLoginStateInnerAsync(ApplicationUser user, CancellationToken cancellationToken = default);
    Task IncreaseUserJwtVersionInnerAsync(ApplicationUser user, CancellationToken cancellationToken = default);
}