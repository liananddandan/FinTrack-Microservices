using IdentityService.Common.DTOs;
using IdentityService.Common.Results;
using IdentityService.Common.Status;
using IdentityService.Domain.Entities;
using IdentityService.Events;
using IdentityService.Repositories.Interfaces;
using IdentityService.Services.Interfaces;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Services;

public class TenantService(
    IUnitOfWork unitOfWork, 
    ITenantRepo tenantRepo,
    IUserDomainService userDomainService,
    IMediator mediator) : ITenantService
{
    public async Task<ServiceResult<RegisterTenantResult>> RegisterTenantAsync(
        string tenantName,
        string adminName, 
        string adminEmail,
        CancellationToken cancellationToken = default
        )
    {
        try
        {
            return await unitOfWork.WithTransactionAsync<ServiceResult<RegisterTenantResult>>(async () =>
            {
                // 1. create tenant
                var tenant = new Tenant
                {
                    Name = tenantName
                };
                await tenantRepo.AddTenantAsync(tenant, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken); 
                // 2. create admin account
                var (user, randomPassword) = await userDomainService
                    .CreateUserOrThrowInnerAsync(adminName, adminEmail, tenant.Id, cancellationToken);

                // 3. create admin role
                var roleName = GetTenantAdminRoleName(tenantName);
                var createStatus = await userDomainService.CreateRoleInnerAsync(roleName, cancellationToken);
                if (createStatus == RoleStatus.CreateFailed)
                {
                    return ServiceResult<RegisterTenantResult>
                        .Fail(ResultCodes.Tenant.RegisterTenantRoleCreateFailed, "Role creation failed");
                }

                // 4. Give user the admin role
                var addStatus = await userDomainService.AddUserToRoleInnerAsync(user, roleName, cancellationToken);
                if (addStatus == RoleStatus.AddRoleToUserFailed)
                {
                    return ServiceResult<RegisterTenantResult>
                        .Fail(ResultCodes.Tenant.RegisterTenantRoleGrantFailed, "Role grant failed");
                }

                // 4. send register tenant event
                await mediator.Publish(new TenantRegisteredEvent(tenant.Id, tenant.PublicId,
                    user.Id, adminName, adminEmail, randomPassword), cancellationToken);

                return ServiceResult<RegisterTenantResult>.Ok(
                    new RegisterTenantResult(tenant.PublicId, adminEmail, randomPassword),
                    ResultCodes.Tenant.RegisterTenantSuccess,
                    "Registered tenant successfully");
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            return ServiceResult<RegisterTenantResult>.Fail(ResultCodes.Tenant.RegisterTenantException, 
                ex.InnerException != null ? ex.InnerException.Message : ex.Message);
        }
    }

    public async Task<ServiceResult<bool>> InviteUserForTenantAsync(string adminPublicId, string adminJwtVersion, string tenantPublicId,
        string adminRoleInTenant, List<string> emails, CancellationToken cancellationToken = default)
    {
        var admin = await userDomainService.GetUserByPublicIdIncludeTenantAsync(adminPublicId, cancellationToken);
        if (admin == null)
        {
            return ServiceResult<bool>.Fail(ResultCodes.User.UserNotFound, "User not found");
        }

        if (admin.Tenant == null || !admin.Tenant.PublicId.ToString().Equals(tenantPublicId))
        {
            return ServiceResult<bool>.Fail(ResultCodes.User.UserTenantInfoMissed, "User tenant not found");
        }

        if (!admin.JwtVersion.ToString().Equals(adminJwtVersion))
        {
            return ServiceResult<bool>.Fail(ResultCodes.Token.JwtTokenVersionInvalid, "Token version is invalid");
        }

        var role = await userDomainService.GetRoleInnerAsync(admin, cancellationToken);
        if (role == null)
        {
            return ServiceResult<bool>.Fail(ResultCodes.User.UserCouldNotFindRole, "User role not found");
        }

        if (!role.Equals(adminRoleInTenant) || !role.Equals(GetTenantAdminRoleName(admin.Tenant.Name)))
        {
            return ServiceResult<bool>.Fail(ResultCodes.User.UserWithoutAdminRolePermission, "User does not have admin role");
        }

        await mediator.Publish(new TenantInvitationEvent(admin, emails), cancellationToken);
        return ServiceResult<bool>.Ok(true, ResultCodes.Tenant.InvitationUsersStartSuccess, "Invitation users start success");
    }

    private string GetTenantAdminRoleName(string tenantName)
    {
        return $"Admin_{tenantName}";
    }
}