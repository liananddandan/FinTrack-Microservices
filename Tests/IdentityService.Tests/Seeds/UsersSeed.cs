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
        await SeedAddLoginTestUser(dbContext, userManager, roleManager);
        await SeedAddFirstLoginTestUser(dbContext, userManager, roleManager);
        await SeedAddSetPasswordTestUser(dbContext, userManager, roleManager);
        await SeedAddRefreshJwtTokenTestUser(dbContext, userManager, roleManager);
        await SeedAddResetPasswordTestUser(dbContext, userManager, roleManager);
        await SeedAddGetUserInfoTestUser(dbContext, userManager, roleManager);
    }

    private static async Task SeedAddLoginTestUser(ApplicationIdentityDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        var tenant = new Tenant() {Name = "TenantForLoginTest" };
        dbContext.Tenants.Add(tenant);
        await dbContext.SaveChangesAsync();
        
        var roleName = $"Admin_{tenant.Name}";
        await roleManager.CreateAsync(new ApplicationRole() { Name = roleName });
        
        var user = new ApplicationUser
        {
            UserName = "TestUserForLogin",
            Email = "testUserForLogin@test.com",
            EmailConfirmed = true,
            IsFirstLogin = false,
            TenantId = tenant.Id,
            SecurityStamp = Guid.NewGuid().ToString(),
        };
        
        var createResult = await userManager.CreateAsync(user, "TestUserForLoginPassword0@");
        if (!createResult.Succeeded)
            throw new Exception("User creation failed: " + string.Join(",", createResult.Errors.Select(e => e.Description)));

        var createdUser = await userManager.FindByEmailAsync(user.Email);
        if (createdUser == null)
            throw new Exception("User not found after creation.");

        await userManager.AddToRoleAsync(createdUser, roleName);
    }
    
    private static async Task SeedAddFirstLoginTestUser(ApplicationIdentityDbContext dbContext, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        var tenant = new Tenant() { Name = "TenantForFirstLoginTest" };
        dbContext.Tenants.Add(tenant);
        
        var roleName = $"Admin_{tenant.Name}";
        await roleManager.CreateAsync(new ApplicationRole() { Name = roleName });
        
        var user = new ApplicationUser
        {
            UserName = "TestUserForFirstLogin",
            Email = "testUserForFirstLogin@test.com",
            EmailConfirmed = true,
            IsFirstLogin = true,
            TenantId = tenant.Id,
            SecurityStamp = Guid.NewGuid().ToString(),
        };
        
                
        var createResult = await userManager.CreateAsync(user, "TestUserForFirstLoginPassword0@");
        if (!createResult.Succeeded)
            throw new Exception("User creation failed: " + string.Join(",", createResult.Errors.Select(e => e.Description)));

        var createdUser = await userManager.FindByEmailAsync(user.Email);
        if (createdUser == null)
            throw new Exception("User not found after creation.");

        await userManager.AddToRoleAsync(createdUser, roleName);
    }

    private static async Task SeedAddSetPasswordTestUser(ApplicationIdentityDbContext dbContext,
        UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        var tenant = new Tenant() { Name = "TenantForChangePasswordTest" };
        dbContext.Tenants.Add(tenant);
        
        var roleName = $"Admin_{tenant.Name}";
        await roleManager.CreateAsync(new ApplicationRole() { Name = roleName });
        
        var user = new ApplicationUser
        {
            UserName = "TestUserForChangePassword",
            Email = "testUserForChangePassword@test.com",
            EmailConfirmed = true,
            IsFirstLogin = true,
            TenantId = tenant.Id,
            SecurityStamp = Guid.NewGuid().ToString(),
        };
        
        var createResult = await userManager.CreateAsync(user, "TestUserForChangePassword0@");
        if (!createResult.Succeeded)
            throw new Exception("User creation failed: " + string.Join(",", createResult.Errors.Select(e => e.Description)));

        var createdUser = await userManager.FindByEmailAsync(user.Email);
        if (createdUser == null)
            throw new Exception("User not found after creation.");

        await userManager.AddToRoleAsync(createdUser, roleName);
    }
    
    private static async Task SeedAddResetPasswordTestUser(ApplicationIdentityDbContext dbContext,
        UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        var tenant = new Tenant() { Name = "TenantForResetPasswordTest" };
        dbContext.Tenants.Add(tenant);
        
        var roleName = $"Admin_{tenant.Name}";
        await roleManager.CreateAsync(new ApplicationRole() { Name = roleName });
        
        var user = new ApplicationUser
        {
            UserName = "TestUserForResetPassword",
            Email = "testUserForResetPassword@test.com",
            EmailConfirmed = true,
            IsFirstLogin = false,
            TenantId = tenant.Id,
            SecurityStamp = Guid.NewGuid().ToString(),
        };
        
        var createResult = await userManager.CreateAsync(user, "TestUserForResetPassword0@");
        if (!createResult.Succeeded)
            throw new Exception("User creation failed: " + string.Join(",", createResult.Errors.Select(e => e.Description)));

        var createdUser = await userManager.FindByEmailAsync(user.Email);
        if (createdUser == null)
            throw new Exception("User not found after creation.");

        await userManager.AddToRoleAsync(createdUser, roleName);
    }
    
    private static async Task SeedAddRefreshJwtTokenTestUser(ApplicationIdentityDbContext dbContext,
        UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        var tenant = new Tenant() { Name = "RefreshJwtTokenTest" };
        dbContext.Tenants.Add(tenant);
        
        var roleName = $"Admin_{tenant.Name}";
        await roleManager.CreateAsync(new ApplicationRole() { Name = roleName });
        
        var user = new ApplicationUser
        {
            UserName = "TestUserForRefreshJwtTokenTest",
            Email = "testUserForRefreshJwtTokenTest@test.com",
            EmailConfirmed = true,
            IsFirstLogin = false,
            TenantId = tenant.Id,
            SecurityStamp = Guid.NewGuid().ToString(),
        };
        
        var createResult = await userManager.CreateAsync(user, "TestUserForRefreshJwtTokenTest0@");
        if (!createResult.Succeeded)
            throw new Exception("User creation failed: " + string.Join(",", createResult.Errors.Select(e => e.Description)));

        var createdUser = await userManager.FindByEmailAsync(user.Email);
        if (createdUser == null)
            throw new Exception("User not found after creation.");

        await userManager.AddToRoleAsync(createdUser, roleName);
    }
    
    private static async Task SeedAddGetUserInfoTestUser(ApplicationIdentityDbContext dbContext,
        UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        var tenant = new Tenant() { Name = "GetUserInfoTest" };
        dbContext.Tenants.Add(tenant);
        
        var roleName = $"Admin_{tenant.Name}";
        await roleManager.CreateAsync(new ApplicationRole() { Name = roleName });
        
        var user = new ApplicationUser
        {
            UserName = "TestUserForGetUserInfoTest",
            Email = "testUserForGetUserInfoTest@test.com",
            EmailConfirmed = true,
            IsFirstLogin = false,
            TenantId = tenant.Id,
            SecurityStamp = Guid.NewGuid().ToString(),
        };
        
        var createResult = await userManager.CreateAsync(user, "TestUserForGetUserInfoTest0@");
        if (!createResult.Succeeded)
            throw new Exception("User creation failed: " + string.Join(",", createResult.Errors.Select(e => e.Description)));

        var createdUser = await userManager.FindByEmailAsync(user.Email);
        if (createdUser == null)
            throw new Exception("User not found after creation.");

        await userManager.AddToRoleAsync(createdUser, roleName);
    }
}