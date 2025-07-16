using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Tests.Seeds;

public class UsersSeed
{
    public static async Task InitialAllDataAsync(ApplicationIdentityDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        await SeedAddAdminTestUser(dbContext, userManager, roleManager, "GetUserInfo");
        await SeedAddAdminTestUser(dbContext, userManager, roleManager, "ResetPassword");
        await SeedAddAdminTestUser(dbContext, userManager, roleManager, "SetPassword");
        await SeedAddAdminTestUser(dbContext, userManager, roleManager, "RefreshJwtToken");
        await SeedAddAdminTestUser(dbContext, userManager, roleManager, "FirstLogin");
        await SeedAddAdminTestUser(dbContext, userManager, roleManager, "Login");
        await SeedAddAdminTestUser(dbContext, userManager, roleManager, "InviteUser");
    }
    
    private static async Task SeedAddAdminTestUser(ApplicationIdentityDbContext dbContext,
        UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager,
        string tenantName)
    {
        var tenant = new Tenant() { Name = $"{tenantName}" };
        dbContext.Tenants.Add(tenant);
        
        var roleName = $"Admin_{tenant.Name}";
        await roleManager.CreateAsync(new ApplicationRole() { Name = $"Admin_{tenant.Name}" });
        
        var user = new ApplicationUser
        {
            UserName = $"TestUserFor{tenantName}",
            Email = $"testUserFor{tenantName}Test@test.com",
            EmailConfirmed = true,
            IsFirstLogin = false,
            TenantId = tenant.Id,
            SecurityStamp = Guid.NewGuid().ToString(),
        };

        if (tenantName.Equals("FirstLogin"))
        {
            user.IsFirstLogin = true;
        }

        var createResult = await userManager.CreateAsync(user, $"TestUserFor{tenantName}0@");
        if (!createResult.Succeeded)
            throw new Exception("User creation failed: " + string.Join(",", createResult.Errors.Select(e => e.Description)));

        var createdUser = await userManager.FindByEmailAsync(user.Email);
        if (createdUser == null)
            throw new Exception("User not found after creation.");

        await userManager.AddToRoleAsync(createdUser, roleName);
    }
}