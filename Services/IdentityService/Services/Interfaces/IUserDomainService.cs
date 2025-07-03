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
}