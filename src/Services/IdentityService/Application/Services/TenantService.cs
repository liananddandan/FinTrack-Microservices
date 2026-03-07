using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Services.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using IdentityService.Infrastructure.Persistence.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SharedKernel.Common.Constants;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;
using StackExchange.Redis;

namespace IdentityService.Application.Services;

public class TenantService(
    ILogger<TenantService> logger,
    IUnitOfWork unitOfWork,
    ITenantRepo tenantRepo,
    IApplicationUserRepo applicationUserRepo,
    UserManager<ApplicationUser> userManager,
    ITenantMembershipRepo tenantMembershipRepo,
    IConnectionMultiplexer redis,
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

    public async Task<ServiceResult<List<TenantMemberDto>>> GetTenantMembersAsync(
        string tenantPublicId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantPublicId))
        {
            return ServiceResult<List<TenantMemberDto>>.Fail(
                ResultCodes.Tenant.GetTenantMembersParameterError,
                "Tenant public id is required.");
        }

        try
        {
            var memberships = await tenantMembershipRepo.GetMembershipsByTenantPublicIdAsync(
                tenantPublicId,
                cancellationToken);

            var result = memberships
                .Select(m => new TenantMemberDto(
                    m.PublicId.ToString(),
                    m.User.PublicId.ToString(),
                    m.User.Email ?? string.Empty,
                    m.User.UserName ?? string.Empty,
                    m.Role.ToString(),
                    m.IsActive,
                    m.JoinedAt))
                .ToList();

            return ServiceResult<List<TenantMemberDto>>.Ok(
                result,
                ResultCodes.Tenant.GetTenantMembersSuccess,
                "Tenant members fetched successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get tenant members for tenant {TenantPublicId}", tenantPublicId);

            return ServiceResult<List<TenantMemberDto>>.Fail(
                ResultCodes.Tenant.GetTenantMembersException,
                "Failed to get tenant members.");
        }
    }
    
    public async Task<ServiceResult<bool>> RemoveTenantMemberAsync(
        string tenantPublicId,
        string membershipPublicId,
        string operatorUserPublicId,
        CancellationToken cancellationToken = default)
    {
        var membership = await tenantMembershipRepo.GetByPublicIdAsync(
            membershipPublicId,
            cancellationToken);

        if (membership == null)
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Tenant.MemberNotFound,
                "Membership not found.");
        }

        if (!membership.IsActive)
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Tenant.MemberAlreadyRemoved,
                "Member already removed.");
        }

        if (membership.Tenant.PublicId.ToString() != tenantPublicId)
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Tenant.MemberNotInTenant,
                "Member does not belong to this tenant.");
        }

        if (membership.User.PublicId.ToString() == operatorUserPublicId)
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Tenant.CannotRemoveSelf,
                "You cannot remove yourself.");
        }

        membership.IsActive = false;
        membership.LeftAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var redisKey = Constant.Redis.JwtVersionPrefix + membership.User.PublicId.ToString();         
            var db = redis.GetDatabase();
            await db.StringIncrementAsync(redisKey);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to increment jwtVersion for user {UserPublicId}",
                membership.User.PublicId);
        }

        return ServiceResult<bool>.Ok(
            true,
            ResultCodes.Tenant.MemberRemoved,
            "Member removed successfully.");
    }
}