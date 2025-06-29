using System.Security.Cryptography;
using IdentityService.Commands;
using IdentityService.Domain.Entities;
using IdentityService.DTOs;
using IdentityService.Events;
using IdentityService.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.CommandHandlers;

public class RegisterTenantCommandHandler(
    ApplicationIdentityDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IMediator mediator
) : IRequestHandler<RegisterTenantCommand, RegisterTenantResult>
{
    private readonly ApplicationIdentityDbContext _dbContext = dbContext;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly IMediator _mediator = mediator;

    public async Task<RegisterTenantResult> Handle(RegisterTenantCommand request, CancellationToken cancellationToken)
    {
        await using var transaction =  await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // 1. create tenant
            var tenant = new Tenant
            {
                Name = request.TenantName
            };
            _dbContext.Tenants.Add(tenant);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            // 2. create admin account
            var randomPassword = GenerateSecurePassword();
            var adminUser = new ApplicationUser
            {
                UserName = request.AdminName,
                Email = request.AdminEmail,
                EmailConfirmed = false,
                TenantId = tenant.Id
            };

            var result = await _userManager.CreateAsync(adminUser, randomPassword);
            if (!result.Succeeded)
            {
                throw new Exception($"Sorry, Could not create admin user, {string.Join(',', result.Errors.Select(e => e.Description))}.");
            }
            
            // 3. create admin role
            var roleName = "Admin";
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new ApplicationRole() { Name = roleName });
            }
            
            await _userManager.AddToRoleAsync(adminUser, roleName);
            
            // 4. send register tenant event
            await _mediator.Publish(new TenantRegisteredEvent(tenant.Id, tenant.PublicId, 
                adminUser.Id, adminUser.Email, randomPassword), cancellationToken);
            
            // 5. return result
            return new RegisterTenantResult(tenant.PublicId, adminUser.Email, randomPassword);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    private string GenerateSecurePassword()
    {
        const int length = 12;
        const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";
        return new string(Enumerable.Range(0, length)
            .Select(_ => valid[RandomNumberGenerator.GetInt32(valid.Length)]).ToArray());
    }
}