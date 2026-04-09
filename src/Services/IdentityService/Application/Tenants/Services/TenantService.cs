using IdentityService.Application.Accounts.Abstractions;
using IdentityService.Application.Accounts.Events;
using IdentityService.Application.Common.Abstractions;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Tenants.Abstractions;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SharedKernel.Common.Constants;
using SharedKernel.Common.Results;
using SharedKernel.Contracts.AuditLogs;
using SharedKernel.Topics;
using StackExchange.Redis;

namespace IdentityService.Application.Tenants.Services;

public class TenantService(
    ILogger<TenantService> logger,
    IUnitOfWork unitOfWork,
    ITenantRepository tenantRepository,
    IApplicationUserRepo applicationUserRepo,
    UserManager<ApplicationUser> userManager,
    ITenantMembershipRepo tenantMembershipRepo,
    IConnectionMultiplexer redis,
    IAuditLogPublisher auditLogPublisher,
    ITurnstileValidationService turnstileValidationService,
    IEmailThrottleService emailThrottleService,
    IMediator mediator,
    IEmailVerificationService emailVerificationService) : ITenantService
{
    public async Task<ServiceResult<RegisterTenantDto>> RegisterTenantAsync(
        string tenantName,
        string adminName,
        string adminEmail,
        string adminPassword,
        string turnstileToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantName))
        {
            return ServiceResult<RegisterTenantDto>.Fail(
                ResultCodes.TenantCodes.RegisterTenantParameterError, "Tenant name is required.");
        }

        if (string.IsNullOrWhiteSpace(adminName))
        {
            return ServiceResult<RegisterTenantDto>.Fail(
                ResultCodes.TenantCodes.RegisterTenantParameterError, "Admin name is required.");
        }

        if (string.IsNullOrWhiteSpace(adminEmail))
        {
            return ServiceResult<RegisterTenantDto>.Fail(
                ResultCodes.TenantCodes.RegisterTenantParameterError, "Admin email is required.");
        }

        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            return ServiceResult<RegisterTenantDto>.Fail(
                ResultCodes.TenantCodes.RegisterTenantParameterError, "Admin password is required.");
        }
        
        if (string.IsNullOrWhiteSpace(turnstileToken))
        {
            return ServiceResult<RegisterTenantDto>.Fail(
                "TURNSTILE_TOKEN_REQUIRED",
                "Verification challenge is required.");
        }
        
        tenantName = tenantName.Trim();
        adminName = adminName.Trim();
        adminEmail = adminEmail.Trim().ToLowerInvariant();
        turnstileToken = turnstileToken.Trim();
        
        var turnstileResult = await turnstileValidationService.ValidateAsync(
            turnstileToken, cancellationToken);
        if (!turnstileResult.Success)
        {
            return ServiceResult<RegisterTenantDto>.Fail(
                turnstileResult.Code ?? "TURNSTILE_VERIFY_FAILED",
                turnstileResult.Message ?? "Verification failed.");
        }
        
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var tenantExists = await tenantRepository.IsTenantNameExistsAsync(tenantName, cancellationToken);
            if (tenantExists)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ServiceResult<RegisterTenantDto>.Fail(
                    ResultCodes.TenantCodes.RegisterTenantExistedError, "Tenant name already exists.");
            }

            var emailExists = await applicationUserRepo.IsEmailExistsAsync(adminEmail, cancellationToken);
            if (emailExists)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ServiceResult<RegisterTenantDto>.Fail(
                    ResultCodes.TenantCodes.RegisterTenantExistedError, "Admin email already exists.");
            }

            var tenant = new Tenant
            {
                Name = tenantName
            };

            await tenantRepository.AddTenantAsync(tenant, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var user = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = false
            };

            var createUserResult = await userManager.CreateAsync(user, adminPassword);
            if (!createUserResult.Succeeded)
            {
                var error = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ServiceResult<RegisterTenantDto>.Fail(
                    ResultCodes.TenantCodes.RegisterTenantCreateError, error);
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

            var sendAllowedResult = await emailThrottleService.CheckRegistrationEmailSendAllowedAsync(
                cancellationToken);
            string message;
            if (!sendAllowedResult.Success)
            {
                logger.LogWarning(
                    "Tenant {TenantId} created successfully, but admin verification email was throttled. Code: {Code}, Message: {Message}",
                    tenant.Id,
                    sendAllowedResult.Code,
                    sendAllowedResult.Message);

                message =
                    "Tenant created successfully. Verification email was temporarily delayed. Please log in later to resend verification email.";
            }
            else
            {
                var verificationResult = await emailVerificationService.CreateTokenAsync(
                    user.Id,
                    createdByIp: null,
                    cancellationToken: cancellationToken);

                if (!verificationResult.Success || verificationResult.Data is null)
                {
                    logger.LogWarning(
                        "Tenant {TenantId} created successfully, but failed to create email verification token for admin user {UserId}.",
                        tenant.Id,
                        user.Id);

                    message =
                        "Tenant created successfully. Verification email could not be sent right now. Please log in later to resend verification email.";
                }
                else
                {
                    await mediator.Publish(
                        new SendEmailVerificationRequestedEvent(
                            user.Id,
                            user.Email!,
                            adminName,
                            verificationResult.Data.RawToken,
                            verificationResult.Data.ExpiresAtUtc),
                        cancellationToken);

                    await emailThrottleService.MarkRegistrationEmailSentAsync(cancellationToken);

                    message =
                        "Tenant created successfully. Please verify the admin email before accessing the workspace.";
                }
            }
            return ServiceResult<RegisterTenantDto>.Ok(
                new RegisterTenantDto(
                    tenant.PublicId.ToString(),
                    user.PublicId.ToString(),
                    user.Email!
                ),
                ResultCodes.TenantCodes.RegisterTenantSuccess,
                message);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);

            logger.LogError(ex,
                "Failed to register tenant {TenantName} with admin {AdminEmail}.",
                tenantName,
                adminEmail);

            return ServiceResult<RegisterTenantDto>.Fail(
                ResultCodes.TenantCodes.RegisterTenantException, "Tenant registration failed.");
        }
    }

    public async Task<ServiceResult<List<TenantMemberDto>>> GetTenantMembersAsync(
        string tenantPublicId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantPublicId))
        {
            return ServiceResult<List<TenantMemberDto>>.Fail(
                ResultCodes.TenantCodes.GetTenantMembersParameterError,
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
                ResultCodes.TenantCodes.GetTenantMembersSuccess,
                "Tenant members fetched successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get tenant members for tenant {TenantPublicId}", tenantPublicId);

            return ServiceResult<List<TenantMemberDto>>.Fail(
                ResultCodes.TenantCodes.GetTenantMembersException,
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
                ResultCodes.TenantCodes.MemberNotFound,
                "Membership not found.");
        }

        if (!membership.IsActive)
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.TenantCodes.MemberAlreadyRemoved,
                "Member already removed.");
        }

        if (membership.Tenant.PublicId.ToString() != tenantPublicId)
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.TenantCodes.MemberNotInTenant,
                "Member does not belong to this tenant.");
        }

        if (membership.User.PublicId.ToString() == operatorUserPublicId)
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.TenantCodes.CannotRemoveSelf,
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

        await auditLogPublisher.PublishAsync(
            AuditLogTopics.MembershipRemoved,
            new AuditLogMessage
            {
                TenantPublicId = tenantPublicId,
                ActorUserPublicId = operatorUserPublicId,
                ActorDisplayName = operatorUserPublicId,
                ActionType = "Membership.Removed",
                Category = "Membership",
                TargetType = "Membership",
                TargetPublicId = membership.PublicId.ToString(),
                TargetDisplay = membership.User.Email,
                Source = "IdentityService",
                OccurredAtUtc = DateTime.UtcNow,
                Metadata =
                [
                    new AuditMetadataItem("role", membership.Role.ToString()),
                    new AuditMetadataItem("isActive", "false")
                ]
            },
            cancellationToken);

        return ServiceResult<bool>.Ok(
            true,
            ResultCodes.TenantCodes.MemberRemoved,
            "Member removed successfully.");
    }

    public async Task<ServiceResult<bool>> ChangeTenantMemberRoleAsync(
        string tenantPublicId,
        string membershipPublicId,
        string operatorUserPublicId,
        string role,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantPublicId))
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.TenantCodes.ChangeMemberRoleParameterError,
                "Tenant public id is required.");
        }

        if (string.IsNullOrWhiteSpace(membershipPublicId))
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.TenantCodes.ChangeMemberRoleParameterError,
                "Membership public id is required.");
        }

        if (string.IsNullOrWhiteSpace(operatorUserPublicId))
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.TenantCodes.ChangeMemberRoleParameterError,
                "Operator user public id is required.");
        }

        if (!Enum.TryParse<TenantRole>(role, true, out var targetRole) ||
            (targetRole != TenantRole.Admin && targetRole != TenantRole.Member))
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.TenantCodes.ChangeMemberRoleInvalidRole,
                "Invalid role.");
        }

        var membership = await tenantMembershipRepo.GetByPublicIdAsync(
            membershipPublicId,
            cancellationToken);

        if (membership == null)
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.TenantCodes.MemberNotFound,
                "Membership not found.");
        }

        if (membership.Tenant.PublicId.ToString() != tenantPublicId)
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.TenantCodes.MemberNotInTenant,
                "Member does not belong to this tenant.");
        }

        if (!membership.IsActive)
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.TenantCodes.ChangeMemberRoleInactiveMembership,
                "Cannot change role for an inactive member.");
        }

        if (membership.User.PublicId.ToString() == operatorUserPublicId)
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.TenantCodes.CannotChangeOwnRole,
                "You cannot change your own role.");
        }

        if (membership.Role == targetRole)
        {
            return ServiceResult<bool>.Ok(
                true,
                ResultCodes.TenantCodes.ChangeMemberRoleNoChange,
                "Member role is already set to the requested value.");
        }

        if (membership.Role == TenantRole.Admin && targetRole == TenantRole.Member)
        {
            var activeAdminCount = await tenantMembershipRepo.CountActiveAdminsAsync(
                membership.TenantId,
                cancellationToken);

            if (activeAdminCount <= 1)
            {
                return ServiceResult<bool>.Fail(
                    ResultCodes.TenantCodes.CannotDemoteLastAdmin,
                    "You cannot demote the last admin of the tenant.");
            }
        }
        
        var oldRole = membership.Role;
        membership.Role = targetRole;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var redisKey = Constant.Redis.JwtVersionPrefix + membership.User.PublicId.ToString();
            var db = redis.GetDatabase();
            await db.StringIncrementAsync(redisKey);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Failed to increment jwtVersion for user {UserPublicId} after role change.",
                membership.User.PublicId);
        }

        await auditLogPublisher.PublishAsync(
            AuditLogTopics.MembershipRoleChanged,
            new AuditLogMessage
            {
                TenantPublicId = tenantPublicId,
                ActorUserPublicId = operatorUserPublicId,
                ActorDisplayName = operatorUserPublicId,
                ActionType = "Membership.RoleChanged",
                Category = "Membership",
                TargetType = "Membership",
                TargetPublicId = membership.PublicId.ToString(),
                TargetDisplay = membership.User.Email,
                Source = "IdentityService",
                OccurredAtUtc = DateTime.UtcNow,
                Metadata =
                [
                    new AuditMetadataItem("oldRole", oldRole.ToString()),
                    new AuditMetadataItem("newRole", targetRole.ToString())
                ]
            },
            cancellationToken);
        
        return ServiceResult<bool>.Ok(
            true,
            ResultCodes.TenantCodes.ChangeMemberRoleSuccess,
            "Member role updated successfully.");
    }
}