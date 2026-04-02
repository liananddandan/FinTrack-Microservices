using IdentityService.Application.Accounts.Abstractions;
using IdentityService.Application.Common.Abstractions;
using IdentityService.Application.Dev.Abstractions;
using IdentityService.Application.Tenants.Abstractions;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using SharedKernel.Contracts.Dev;

namespace IdentityService.Application.Dev.Services;

public class DevSeedService(
    IUnitOfWork unitOfWork,
    ITenantRepository tenantRepository,
    IApplicationUserRepo applicationUserRepo,
    ITenantMembershipRepo tenantMembershipRepo,
    UserManager<ApplicationUser> userManager) : IDevSeedService
{
    public async Task<DevIdentitySeedResult> SeedIdentityAsync(
        DevIdentitySeedRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.TenantName))
        {
            throw new InvalidOperationException("TenantName is required.");
        }

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var tenantName = request.TenantName.Trim();

            var tenant = await tenantRepository.GetTenantByNameAsync(tenantName, cancellationToken);
            if (tenant is null)
            {
                tenant = new Tenant
                {
                    Name = tenantName
                };

                await tenantRepository.AddTenantAsync(tenant, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            var admin = await EnsureUserAsync(
                request.AdminEmail,
                request.AdminPassword,
                cancellationToken);

            var member = await EnsureUserAsync(
                request.MemberEmail,
                request.MemberPassword,
                cancellationToken);

            await EnsureMembershipAsync(
                tenant.Id,
                admin.Id,
                TenantRole.Admin,
                cancellationToken);

            await EnsureMembershipAsync(
                tenant.Id,
                member.Id,
                TenantRole.Member,
                cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return new DevIdentitySeedResult(
                tenant.PublicId.ToString(),
                tenant.Name,
                admin.PublicId.ToString(),
                request.AdminEmail.Trim().ToLowerInvariant(),
                request.AdminPassword,
                member.PublicId.ToString(),
                request.MemberEmail.Trim().ToLowerInvariant(),
                request.MemberPassword);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task<ApplicationUser> EnsureUserAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await applicationUserRepo.GetUserByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = normalizedEmail,
                Email = normalizedEmail,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }

            return user;
        }

        var needsUpdate = false;

        if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            needsUpdate = true;
        }

        if (string.IsNullOrWhiteSpace(user.UserName))
        {
            user.UserName = normalizedEmail;
            needsUpdate = true;
        }

        if (needsUpdate)
        {
            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ", updateResult.Errors.Select(e => e.Description)));
            }
        }

        var hasExpectedPassword = await userManager.CheckPasswordAsync(user, password);
        if (hasExpectedPassword)
        {
            return user;
        }

        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var resetResult = await userManager.ResetPasswordAsync(user, resetToken, password);
        if (!resetResult.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(", ", resetResult.Errors.Select(e => e.Description)));
        }

        return user;
    }

    private async Task EnsureMembershipAsync(
        long tenantId,
        long userId,
        TenantRole role,
        CancellationToken cancellationToken)
    {
        var membership = await tenantMembershipRepo.GetAnyMembershipAsync(
            tenantId,
            userId,
            cancellationToken);

        if (membership is null)
        {
            await tenantMembershipRepo.AddMembershipAsync(new TenantMembership
            {
                TenantId = tenantId,
                UserId = userId,
                Role = role,
                IsActive = true,
                JoinedAt = DateTime.UtcNow
            }, cancellationToken);

            return;
        }

        membership.Role = role;
        membership.IsActive = true;
        membership.LeftAt = null;
    }
}