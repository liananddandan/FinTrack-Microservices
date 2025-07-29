using IdentityService.Common.DTOs;
using IdentityService.Common.Status;
using IdentityService.Domain.Entities;
using IdentityService.Events;
using IdentityService.Repositories.Interfaces;
using IdentityService.Services.Interfaces;
using MediatR;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;

namespace IdentityService.Services;

public class TenantService(
    IUnitOfWork unitOfWork,
    ITenantRepo tenantRepo,
    IUserDomainService userDomainService,
    ITenantInvitationService tenantInvitationService,
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

                var role = new ApplicationRole() { Name = GetTenantAdminRoleName(tenantName)};
                var roleStatus = await userDomainService.CreateRoleInnerAsync(role, cancellationToken);
                if (roleStatus == RoleStatus.CreateFailed)
                {
                    return ServiceResult<RegisterTenantResult>
                        .Fail(ResultCodes.Tenant.RegisterTenantRoleCreateFailed, "Role creation failed");
                }

                // 2. create admin account
                var (user, randomPassword) = await userDomainService
                    .CreateUserOrThrowInnerAsync(adminName, adminEmail, tenant.Id, role.Id, cancellationToken);
                
                // 4. Give user the admin role
                var addStatus = await userDomainService.AddUserToRoleInnerAsync(user, role.Name, cancellationToken);
                if (addStatus == RoleStatus.AddRoleToUserFailed)
                {
                    return ServiceResult<RegisterTenantResult>
                        .Fail(ResultCodes.Tenant.RegisterTenantRoleGrantFailed, "Role grant failed");
                }

                // 4. send register tenant event
                await mediator.Publish(new UserRegisteredEvent(tenant.Id, tenant.PublicId,
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

    public async Task<ServiceResult<bool>> InviteUserForTenantAsync(string adminPublicId,
        string tenantPublicId,
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

        var role = await userDomainService.GetUserRoleInnerAsync(admin, cancellationToken);
        if (role == null)
        {
            return ServiceResult<bool>.Fail(ResultCodes.User.UserCouldNotFindRole, "User role not found");
        }

        if (!role.Equals(adminRoleInTenant) || !role.Equals(GetTenantAdminRoleName(admin.Tenant.Name)))
        {
            return ServiceResult<bool>.Fail(ResultCodes.User.UserWithoutAdminRolePermission,
                "User does not have admin role");
        }

        await mediator.Publish(new TenantInvitationEvent(admin, emails), cancellationToken);
        return ServiceResult<bool>.Ok(true, ResultCodes.Tenant.InvitationUsersStartSuccess,
            "Invitation users start success");
    }

    public async Task<ServiceResult<bool>> ReceiveInviteForTenantAsync(string invitationPublicId, CancellationToken cancellationToken = default)
    {
        var tenantInvitationResult =
            await tenantInvitationService.GetTenantInvitationByPublicIdAsync(invitationPublicId, cancellationToken);
        if (!tenantInvitationResult.Success)
        {
            return ServiceResult<bool>.Fail(tenantInvitationResult.Code!, tenantInvitationResult.Message!);
        }

        var invitation = tenantInvitationResult.Data!;
        if (invitation.ExpiredAt < DateTime.UtcNow)
        {
            return ServiceResult<bool>.Fail(ResultCodes.Tenant.InvitationExpired, "Invitation expired");
        }

        switch (invitation.Status)
        {
            case InvitationStatus.Revoked:
                return ServiceResult<bool>.Fail(ResultCodes.Tenant.InvitationRevoked,
                    "Invitation status is revoked");
            case InvitationStatus.Accepted:
                return ServiceResult<bool>.Fail(ResultCodes.Tenant.InvitationHasAccepted,
                    "Invitation status is accepted");
        }

        var tenant = await tenantRepo.GetTenantByPublicIdAsync(invitation.TenantPublicId, cancellationToken);
        if (tenant == null)
        {
            return ServiceResult<bool>.Fail(ResultCodes.Tenant.InvitationWithAInvalidTenant, "Tenant not found");
        }

        var role = await userDomainService.GetRoleByNameInnerAsync(invitation.Role, cancellationToken);
        if (role == null)
        {
            return ServiceResult<bool>.Fail(ResultCodes.Tenant.InvitationUsingWrongRole, "Role not found");
        }

        try
        {
            var createResult = await unitOfWork.WithTransactionAsync(async () =>
            {
                // create a new user
                var (user, password) = await userDomainService
                    .CreateUserOrThrowInnerAsync(invitation.Email, invitation.Email, tenant.Id, role.Id,
                        cancellationToken);
                
                // create a role for user
                var roleName = invitation.Role;
                var roleExist = await userDomainService.IsRoleExistAsync(roleName, cancellationToken);
                if (roleExist != RoleStatus.RoleAlreadyExist)
                {
                    var roleCreateResult = await userDomainService.CreateRoleInnerAsync(roleName, cancellationToken);
                    if (roleCreateResult != RoleStatus.CreateSuccess)
                    {
                        return ServiceResult<bool>.Fail(ResultCodes.User.RoleCreatedFailed, "Role could not created");
                    }
                }
                var addRoleResult = await userDomainService.AddUserToRoleInnerAsync(user, roleName, cancellationToken);
                if (addRoleResult != RoleStatus.AddRoleToUserSuccess)
                {
                    return ServiceResult<bool>.Fail(ResultCodes.User.RoleGrantUserFailed, "Role could not grant to user");
                }
                await mediator.Publish(new UserRegisteredEvent(tenant.Id, tenant.PublicId,
                    user.Id, user.UserName!, user.Email!, password), cancellationToken);
                return ServiceResult<bool>.Ok(true, ResultCodes.User.UserRegisterSuccess, "User register success");
            }, cancellationToken);
            if (!createResult.Success)
            {
                return ServiceResult<bool>.Fail(createResult.Code!, createResult.Message!);
            }

            invitation.Status = InvitationStatus.Accepted;
            invitation.AcceptedAt = DateTime.UtcNow;
            var updateResult = await tenantInvitationService.UpdateTenantInvitationAsync(invitation, cancellationToken);
            return updateResult.Success
                ? ServiceResult<bool>.Ok(true, ResultCodes.Tenant.InvitationReceiveSuccess,
                    "Invitation receive success")
                : ServiceResult<bool>.Fail(updateResult.Code!, updateResult.Message!);
        }
        catch
        {
            return ServiceResult<bool>.Fail(ResultCodes.Tenant.InvitationCreateFailed, "Invitation create failed");
        }
    }

    public async Task<ServiceResult<IEnumerable<UserInfoDto>>> GetUsersForTenantAsync(string adminPublicId, string tenantPublicId,
        string adminRoleInTenant, CancellationToken cancellationToken = default)
    {
        var user = await userDomainService.GetUserByPublicIdIncludeTenantAndRoleAsync(adminPublicId, cancellationToken);
        if (user == null)
        {
            return ServiceResult<IEnumerable<UserInfoDto>>.Fail(ResultCodes.User.UserNotFound, "User not found");
        }
        var role = user.Role;
        if (role == null 
            || role.Name == null 
            || !role.Name.Equals(adminRoleInTenant) 
            || !role.Name.Equals(GetTenantAdminRoleName(user.Tenant.Name)))
        {
            return ServiceResult<IEnumerable<UserInfoDto>>.Fail(ResultCodes.Tenant.UserDoNotHavePermissionToQueryTenant, "User do not have permission to query tenant");
        }

        if (user.Tenant == null || !user.Tenant.PublicId.ToString().Equals(tenantPublicId))
        {
            return ServiceResult<IEnumerable<UserInfoDto>>.Fail(ResultCodes.Tenant.UserDoNotBelongToTenant, "User do not belong to tenant");
        }
        
        var users = await userDomainService.GetAllUsersInTenantIncludeRoleAsync(user.TenantId, cancellationToken);
        var userInfoDtos = users.Select(u => new UserInfoDto()
        {
            email = u.Email,
            userName = u.UserName,
            userPublicId = u.PublicId.ToString(),
            tenantInfoDto = null,
            roleName = u.Role.Name,
        }).ToList();
        return ServiceResult<IEnumerable<UserInfoDto>>.Ok(userInfoDtos, ResultCodes.Tenant.GetAllUserInTenantSuccess, "Get all users in tenant success");
    }

    private string GetTenantAdminRoleName(string tenantName)
    {
        return $"Admin_{tenantName}";
    }
}