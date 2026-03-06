using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Services.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using IdentityService.Infrastructure.Persistence.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Services;

public class TenantService(
    ILogger<TenantService> logger,
    IUnitOfWork unitOfWork,
    ITenantRepo tenantRepo,
    IApplicationUserRepo applicationUserRepo,
    UserManager<ApplicationUser> userManager,
    ITenantMembershipRepo tenantMembershipRepo,
    IMediator mediator) : ITenantService
{
    public async Task<ServiceResult<RegisterTenantResult>> RegisterTenantAsync(
        string tenantName,
        string adminName,
        string adminEmail,
        string adminPassword,
        CancellationToken cancellationToken = default)
    {
        tenantName = tenantName.Trim();
        adminName = adminName.Trim();
        adminEmail = adminEmail.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(tenantName))
        {
            return ServiceResult<RegisterTenantResult>.Fail(
                ResultCodes.Tenant.RegisterTenantParameterError, "Tenant name is required.");
        }

        if (string.IsNullOrWhiteSpace(adminName))
        {
            return ServiceResult<RegisterTenantResult>.Fail(
                ResultCodes.Tenant.RegisterTenantParameterError, "Admin name is required.");
        }

        if (string.IsNullOrWhiteSpace(adminEmail))
        {
            return ServiceResult<RegisterTenantResult>.Fail(
                ResultCodes.Tenant.RegisterTenantParameterError, "Admin email is required.");
        }

        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            return ServiceResult<RegisterTenantResult>.Fail(
                ResultCodes.Tenant.RegisterTenantParameterError, "Admin password is required.");
        }

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var tenantExists = await tenantRepo.IsTenantNameExistsAsync(tenantName, cancellationToken);
            if (tenantExists)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ServiceResult<RegisterTenantResult>.Fail(
                    ResultCodes.Tenant.RegisterTenantExistedError, "Tenant name already exists.");
            }

            var emailExists = await applicationUserRepo.IsEmailExistsAsync(adminEmail, cancellationToken);
            if (emailExists)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ServiceResult<RegisterTenantResult>.Fail(
                    ResultCodes.Tenant.RegisterTenantExistedError, "Admin email already exists.");
            }

            var tenant = new Tenant
            {
                Name = tenantName
            };

            await tenantRepo.AddTenantAsync(tenant, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var user = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var createUserResult = await userManager.CreateAsync(user, adminPassword);
            if (!createUserResult.Succeeded)
            {
                var error = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ServiceResult<RegisterTenantResult>.Fail(
                    ResultCodes.Tenant.RegisterTenantCreateError, error);
            }

            var membership = new TenantMembership
            {
                UserId = user.Id,
                TenantId = tenant.Id,
                Role = TenantRole.Admin,
                IsActive = true,
                JoinedAt = DateTime.UtcNow
            };

            await tenantMembershipRepo.AddMembershipAsync(membership, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return ServiceResult<RegisterTenantResult>.Ok(
                new RegisterTenantResult(
                    tenant.PublicId.ToString(),
                    user.PublicId.ToString(),
                    user.Email!
                ),
                ResultCodes.Tenant.RegisterTenantSuccess,
                "Tenant created successfully.");
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);

            logger.LogError(ex,
                "Failed to register tenant {TenantName} with admin {AdminEmail}.",
                tenantName,
                adminEmail);

            return ServiceResult<RegisterTenantResult>.Fail(
                ResultCodes.Tenant.RegisterTenantException, "Tenant registration failed.");
        }
    }

    public Task<ServiceResult<bool>> InviteUserForTenantAsync(string adminPublicId, string tenantPublicId,
        string adminRoleInTenant, List<string> emails,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<bool>> ReceiveInviteForTenantAsync(string invitationPublicId,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<IEnumerable<UserInfoDto>>> GetUsersForTenantAsync(string adminPublicId,
        string tenantPublicId, string adminRoleInTenant,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}