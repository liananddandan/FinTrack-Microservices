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
    IUserDomainService userService,
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
                var (user, randomPassword) = await userService
                    .CreateUserOrThrowInnerAsync(adminName, adminEmail, tenant.Id, cancellationToken);

                // 3. create admin role
                var roleName = $"Admin_{tenantName}";
                var createStatus = await userService.CreateRoleInnerAsync(roleName, cancellationToken);
                if (createStatus == RoleStatus.CreateFailed)
                {
                    return ServiceResult<RegisterTenantResult>
                        .Fail(ResultCodes.Tenant.RegisterTenantRoleCreateFailed, "Role creation failed");
                }

                // 4. Give user the admin role
                var addStatus = await userService.AddUserToRoleInnerAsync(user, roleName, cancellationToken);
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
}