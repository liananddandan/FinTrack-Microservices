using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Tests.Seeds;

public class UsersSeed
{
    public static async Task InitialAllDataAsync(ApplicationIdentityDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        await SeedAddTestAdminUser(dbContext, userManager, roleManager, "GetUserInfo");
        await SeedAddTestAdminUser(dbContext, userManager, roleManager, "ResetPassword");
        await SeedAddTestAdminUser(dbContext, userManager, roleManager, "SetPassword");
        await SeedAddTestAdminUser(dbContext, userManager, roleManager, "RefreshJwtToken");
        await SeedAddTestAdminUser(dbContext, userManager, roleManager, "FirstLogin");
        await SeedAddTestAdminUser(dbContext, userManager, roleManager, "Login");
        await SeedAddTestAdminUser(dbContext, userManager, roleManager, "InviteUser");
        await SeedAddTestAdminUser(dbContext, userManager, roleManager, "GetUsersInTenant");
        await SeedAddTestOrdinaryUser(dbContext, userManager, roleManager, "GetUsersInTenant");
    }
    
    private static async Task SeedAddTestAdminUser(ApplicationIdentityDbContext dbContext,
        UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager,
        string tenantName)
    {
        var tenant = new Tenant() { Name = $"{tenantName}" };
        dbContext.Tenants.Add(tenant);
        
        var roleName = $"Admin_{tenant.Name}";
        var role = new ApplicationRole() { Name = roleName };
        await roleManager.CreateAsync(role);
        
        var user = new ApplicationUser
        {
            UserName = $"TestUserFor{tenantName}",
            Email = $"testUserFor{tenantName}Test@test.com",
            EmailConfirmed = true,
            IsFirstLogin = false,
            TenantId = tenant.Id,
            SecurityStamp = Guid.NewGuid().ToString(),
            RoleId = role.Id
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

    public static async Task SeedAddTestOrdinaryUser(ApplicationIdentityDbContext dbContext,
        UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager,
        string tenantName)
    {
        var tenant = await dbContext.Tenants.Where(t => t.Name == tenantName).FirstOrDefaultAsync();
        
        var roleName = $"User_{tenant.Name}";
        ApplicationRole role = null;
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            role = new ApplicationRole() { Name = roleName };
            await roleManager.CreateAsync(role);
        }
        else
        {
            role = await roleManager.FindByNameAsync(roleName);
        }
        
        var user = new ApplicationUser
        {
            UserName = $"TestUserFor{tenantName}{Guid.NewGuid()}",
            Email = $"testUserFor{tenantName}{Guid.NewGuid()}Test@test.com",
            EmailConfirmed = true,
            IsFirstLogin = false,
            TenantId = tenant.Id,
            SecurityStamp = Guid.NewGuid().ToString(),
            RoleId = role.Id
        };
        
        var createResult = await userManager.CreateAsync(user, $"TestUserFor{tenantName}0@");
        if (!createResult.Succeeded)
            throw new Exception("User creation failed: " + string.Join(",", createResult.Errors.Select(e => e.Description)));

        var createdUser = await userManager.FindByEmailAsync(user.Email);
        if (createdUser == null)
            throw new Exception("User not found after creation.");

        await userManager.AddToRoleAsync(createdUser, roleName);
    }
}