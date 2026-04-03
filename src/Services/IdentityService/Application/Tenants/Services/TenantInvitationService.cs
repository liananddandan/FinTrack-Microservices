using IdentityService.Application.Accounts.Abstractions;
using IdentityService.Application.Common.Abstractions;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Common.Status;
using IdentityService.Application.Tenants.Abstractions;
using IdentityService.Application.Tenants.Events;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using MediatR;
using SharedKernel.Common.Results;
using SharedKernel.Contracts.AuditLogs;
using SharedKernel.Topics;

namespace IdentityService.Application.Tenants.Services;

public class TenantInvitationService(
    ITenantRepository tenantRepository,
    IApplicationUserRepo userRepo,
    ITenantMembershipRepo membershipRepo,
    ITenantInvitationRepo invitationRepo,
    IUnitOfWork unitOfWork,
    ILogger<TenantInvitationService> logger,
    IMediator mediator,
    IAuditLogPublisher auditLogPublisher) : ITenantInvitationService
{
    public async Task<ServiceResult<TenantInvitation>> GetTenantInvitationByPublicIdAsync(
        string publicId,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(publicId, out var parsedPublicId))
        {
            return ServiceResult<TenantInvitation>.Fail(
                ResultCodes.TenantCodes.InvitationInvalidPublicId,
                "Invalid invitation public id.");
        }

        var invitation = await invitationRepo.GetByPublicIdAsync(
            parsedPublicId,
            cancellationToken);

        if (invitation == null)
        {
            return ServiceResult<TenantInvitation>.Fail(
                ResultCodes.TenantCodes.InvitationNotFound,
                "Invitation not found.");
        }

        return ServiceResult<TenantInvitation>.Ok(
            invitation,
            ResultCodes.TenantCodes.InvitationSuccess,
            "Invitation retrieved.");
    }

    public async Task<ServiceResult<TenantInvitation>> GetTenantInvitationByEmailAsync(string email,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<bool>> UpdateTenantInvitationAsync(TenantInvitation invitation,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<bool>> CreateInvitationAsync(
        string tenantPublicId,
        string email,
        string role,
        string invitedByUserPublicId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantPublicId))
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.TenantCodes.CreateInvitationParameterError,
                "Tenant public id is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.TenantCodes.CreateInvitationParameterError,
                "Email is required.");
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.TenantCodes.CreateInvitationParameterError,
                "Role is required.");
        }

        email = email.Trim().ToLowerInvariant();

        logger.LogInformation(
            "CreateInvitationAsync started. TenantPublicId={TenantPublicId}, Email={Email}, Role={Role}, InvitedByUserPublicId={InvitedByUserPublicId}",
            tenantPublicId,
            email,
            role,
            invitedByUserPublicId);

        try
        {
            var tenant = await tenantRepository.GetTenantByPublicIdAsync(
                tenantPublicId,
                cancellationToken);

            if (tenant == null)
            {
                logger.LogWarning(
                    "CreateInvitationAsync failed because tenant was not found. TenantPublicId={TenantPublicId}",
                    tenantPublicId);

                return ServiceResult<bool>.Fail(
                    ResultCodes.TenantCodes.CreateInvitationTenantNotFound,
                    "Tenant not found.");
            }

            logger.LogInformation(
                "Tenant found for invitation. TenantId={TenantId}, TenantPublicId={TenantPublicId}, TenantName={TenantName}",
                tenant.Id,
                tenant.PublicId,
                tenant.Name);

            var user = await userRepo.GetUserByEmailAsync(email, cancellationToken);
            if (user == null)
            {
                logger.LogWarning(
                    "CreateInvitationAsync failed because invited user was not found. TenantPublicId={TenantPublicId}, Email={Email}",
                    tenantPublicId,
                    email);

                return ServiceResult<bool>.Fail(
                    ResultCodes.TenantCodes.CreateInvitationUserNotFound,
                    "User not found.");
            }

            logger.LogInformation(
                "Invited user found. UserId={UserId}, UserPublicId={UserPublicId}, Email={Email}",
                user.Id,
                user.PublicId,
                user.Email);

            var existingMembership = await membershipRepo.GetMembershipAsync(
                tenant.Id,
                user.Id,
                cancellationToken);

            if (existingMembership != null)
            {
                logger.LogWarning(
                    "CreateInvitationAsync blocked because user already belongs to tenant. TenantPublicId={TenantPublicId}, Email={Email}, MembershipPublicId={MembershipPublicId}, IsActive={IsActive}",
                    tenantPublicId,
                    email,
                    existingMembership.PublicId,
                    existingMembership.IsActive);

                return ServiceResult<bool>.Fail(
                    ResultCodes.TenantCodes.CreateInvitationUserAlreadyExists,
                    "User already belongs to this tenant.");
            }

            if (!Guid.TryParse(invitedByUserPublicId, out _))
            {
                logger.LogWarning(
                    "CreateInvitationAsync failed because inviter public id is invalid. InvitedByUserPublicId={InvitedByUserPublicId}",
                    invitedByUserPublicId);

                return ServiceResult<bool>.Fail(
                    ResultCodes.TenantCodes.CreateInvitationParameterError,
                    "Invalid inviter.");
            }

            var inviter = await userRepo.GetUserByPublicIdAsync(
                invitedByUserPublicId,
                cancellationToken);

            if (inviter == null)
            {
                logger.LogWarning(
                    "CreateInvitationAsync failed because inviter was not found. InvitedByUserPublicId={InvitedByUserPublicId}",
                    invitedByUserPublicId);

                return ServiceResult<bool>.Fail(
                    ResultCodes.TenantCodes.CreateInvitationInviterNotFound,
                    "Inviter not found.");
            }

            if (!Enum.TryParse<TenantRole>(role, true, out var parsedRole))
            {
                logger.LogWarning(
                    "CreateInvitationAsync failed because role is invalid. Role={Role}",
                    role);

                return ServiceResult<bool>.Fail(
                    ResultCodes.TenantCodes.CreateInvitationParameterError,
                    "Invalid role.");
            }

            var existingPendingInvitations = await invitationRepo.GetByTenantPublicIdAsync(
                tenantPublicId,
                cancellationToken);

            var duplicatedPendingInvitation = existingPendingInvitations.FirstOrDefault(x =>
                x.Email.ToLower() == email &&
                x.Status == InvitationStatus.Pending &&
                x.ExpiredAt > DateTime.UtcNow);

            if (duplicatedPendingInvitation != null)
            {
                logger.LogWarning(
                    "CreateInvitationAsync blocked because a pending invitation already exists. TenantPublicId={TenantPublicId}, Email={Email}, InvitationPublicId={InvitationPublicId}, ExpiredAt={ExpiredAt}",
                    tenantPublicId,
                    email,
                    duplicatedPendingInvitation.PublicId,
                    duplicatedPendingInvitation.ExpiredAt);

                return ServiceResult<bool>.Fail(
                    ResultCodes.TenantCodes.CreateInvitationAlreadyPending,
                    "A pending invitation already exists for this user.");
            }

            var invitation = new TenantInvitation
            {
                Email = email,
                TenantId = tenant.Id,
                Role = parsedRole,
                Status = InvitationStatus.Pending,
                ExpiredAt = DateTime.UtcNow.AddDays(7),
                Version = 1,
                CreatedByUserId = inviter.Id
            };

            logger.LogInformation(
                "Creating tenant invitation entity. TenantPublicId={TenantPublicId}, Email={Email}, Role={Role}, InviterPublicId={InviterPublicId}",
                tenantPublicId,
                email,
                parsedRole,
                inviter.PublicId);

            await invitationRepo.AddAsync(invitation, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Tenant invitation saved successfully. InvitationPublicId={InvitationPublicId}, TenantPublicId={TenantPublicId}, Email={Email}",
                invitation.PublicId,
                tenantPublicId,
                email);

            logger.LogInformation(
                "Publishing TenantInvitationCreatedEvent. InvitationPublicId={InvitationPublicId}, TenantName={TenantName}, Email={Email}",
                invitation.PublicId,
                tenant.Name,
                email);

            await mediator.Publish(
                new TenantInvitationCreatedEvent(
                    invitation.PublicId.ToString(),
                    tenant.Name,
                    email
                ),
                cancellationToken);

            logger.LogInformation(
                "TenantInvitationCreatedEvent published successfully. InvitationPublicId={InvitationPublicId}, Email={Email}",
                invitation.PublicId,
                email);

            logger.LogInformation(
                "Publishing audit log for invitation creation. InvitationPublicId={InvitationPublicId}, Email={Email}",
                invitation.PublicId,
                invitation.Email);

            await auditLogPublisher.PublishAsync(
                AuditLogTopics.MembershipInvited,
                new AuditLogMessage
                {
                    TenantPublicId = tenantPublicId,
                    ActorUserPublicId = inviter.PublicId.ToString(),
                    ActorDisplayName = inviter.UserName,
                    ActionType = "Membership.Invited",
                    Category = "Membership",
                    TargetType = "Invitation",
                    TargetPublicId = invitation.PublicId.ToString(),
                    TargetDisplay = invitation.Email,
                    Source = "IdentityService",
                    OccurredAtUtc = DateTime.UtcNow,
                    Metadata =
                    [
                        new AuditMetadataItem("role", invitation.Role.ToString()),
                        new AuditMetadataItem("email", invitation.Email)
                    ]
                },
                cancellationToken);

            logger.LogInformation(
                "Invitation audit log published successfully. InvitationPublicId={InvitationPublicId}, Email={Email}",
                invitation.PublicId,
                invitation.Email);

            return ServiceResult<bool>.Ok(
                true,
                ResultCodes.TenantCodes.CreateInvitationSuccess,
                "Invitation created successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to create invitation for {Email} in tenant {TenantPublicId}",
                email,
                tenantPublicId);

            return ServiceResult<bool>.Fail(
                ResultCodes.TenantCodes.CreateInvitationException,
                "Failed to create invitation.");
        }
    }

    public async Task<ServiceResult<ResolveTenantInvitationDto>> ResolveInvitationAsync(
        string invitationPublicId,
        string invitationVersion,
        CancellationToken cancellationToken = default)
    {
        var invitationResult = await GetTenantInvitationByPublicIdAsync(
            invitationPublicId,
            cancellationToken);

        if (!invitationResult.Success || invitationResult.Data == null)
        {
            return ServiceResult<ResolveTenantInvitationDto>.Fail(
                ResultCodes.TenantCodes.ResolveInvitationNotFound,
                "Invitation not found.");
        }

        var invitation = invitationResult.Data;

        if (!int.TryParse(invitationVersion, out var parsedVersion) ||
            parsedVersion != invitation.Version)
        {
            return ServiceResult<ResolveTenantInvitationDto>.Fail(
                ResultCodes.TenantCodes.ResolveInvitationVersionInvalid,
                "Invitation version is invalid.");
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            return ServiceResult<ResolveTenantInvitationDto>.Fail(
                ResultCodes.TenantCodes.ResolveInvitationStatusInvalid,
                "Invitation is no longer available.");
        }

        if (invitation.ExpiredAt <= DateTime.UtcNow)
        {
            return ServiceResult<ResolveTenantInvitationDto>.Fail(
                ResultCodes.TenantCodes.ResolveInvitationExpired,
                "Invitation has expired.");
        }

        var result = new ResolveTenantInvitationDto(
            invitation.PublicId.ToString(),
            invitation.Tenant.Name,
            invitation.Email,
            invitation.Role.ToString(),
            invitation.Status.ToString(),
            invitation.ExpiredAt);

        return ServiceResult<ResolveTenantInvitationDto>.Ok(
            result,
            ResultCodes.TenantCodes.ResolveInvitationSuccess,
            "Invitation resolved successfully.");
    }

    public async Task<ServiceResult<bool>> AcceptInvitationAsync(
        string invitationPublicId,
        string invitationVersion,
        CancellationToken cancellationToken = default)
    {
        var invitationResult = await GetTenantInvitationByPublicIdAsync(
            invitationPublicId,
            cancellationToken);

        if (!invitationResult.Success || invitationResult.Data == null)
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.TenantCodes.AcceptInvitationNotFound,
                "Invitation not found.");
        }

        var invitation = invitationResult.Data;

        if (!int.TryParse(invitationVersion, out var parsedVersion) ||
            parsedVersion != invitation.Version)
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.TenantCodes.AcceptInvitationVersionInvalid,
                "Invitation version is invalid.");
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.TenantCodes.AcceptInvitationStatusInvalid,
                "Invitation is no longer available.");
        }

        if (invitation.ExpiredAt <= DateTime.UtcNow)
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.TenantCodes.AcceptInvitationExpired,
                "Invitation has expired.");
        }

        var user = await userRepo.GetUserByEmailAsync(invitation.Email, cancellationToken);
        if (user == null)
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.TenantCodes.AcceptInvitationUserNotFound,
                "Invited user not found.");
        }

        var existingMembership = await membershipRepo.GetAnyMembershipAsync(
            invitation.TenantId,
            user.Id,
            cancellationToken);
        TenantMembership membership;
        if (existingMembership != null)
        {
            if (existingMembership.IsActive)
            {
                return ServiceResult<bool>.Fail(
                    ResultCodes.TenantCodes.AcceptInvitationMembershipExists,
                    "User already belongs to this tenant.");
            }

            existingMembership.IsActive = true;
            existingMembership.LeftAt = null;
            existingMembership.JoinedAt = DateTime.UtcNow;
            existingMembership.Role = invitation.Role;
            membership = existingMembership;
        }
        else
        {
            membership = new TenantMembership
            {
                TenantId = invitation.TenantId,
                UserId = user.Id,
                Role = invitation.Role,
                IsActive = true,
                JoinedAt = DateTime.UtcNow,
                LeftAt = null
            };

            await membershipRepo.AddMembershipAsync(membership, cancellationToken);
        }

        invitation.Status = InvitationStatus.Accepted;
        invitation.AcceptedAt = DateTime.UtcNow;
        invitation.Version += 1;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogPublisher.PublishAsync(
            AuditLogTopics.MembershipAccepted,
            new AuditLogMessage
            {
                TenantPublicId = invitation.Tenant.PublicId.ToString(),
                ActorUserPublicId = user.PublicId.ToString(),
                ActorDisplayName = user.UserName,
                ActionType = "Membership.Accepted",
                Category = "Membership",
                TargetType = "Membership",
                TargetPublicId = membership.PublicId.ToString(),
                TargetDisplay = user.Email,
                Source = "IdentityService"
            },
            cancellationToken);
        return ServiceResult<bool>.Ok(
            true,
            ResultCodes.TenantCodes.AcceptInvitationSuccess,
            "Invitation accepted successfully.");
    }

    public async Task<ServiceResult<List<TenantInvitationDto>>> GetTenantInvitationsAsync(
        string tenantPublicId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantPublicId))
        {
            return ServiceResult<List<TenantInvitationDto>>.Fail(
                ResultCodes.TenantCodes.GetTenantInvitationsParameterError,
                "Tenant public id is required.");
        }

        try
        {
            var invitations = await invitationRepo.GetByTenantPublicIdAsync(
                tenantPublicId,
                cancellationToken);

            var result = invitations
                .Select(x => new TenantInvitationDto(
                    x.PublicId.ToString(),
                    x.Email,
                    x.Role.ToString(),
                    x.Status.ToString(),
                    x.CreatedAt,
                    x.AcceptedAt,
                    x.ExpiredAt,
                    x.CreatedByUser.Email ?? string.Empty
                ))
                .ToList();

            return ServiceResult<List<TenantInvitationDto>>.Ok(
                result,
                ResultCodes.TenantCodes.GetTenantInvitationsSuccess,
                "Tenant invitations fetched successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to get tenant invitations for tenant {TenantPublicId}",
                tenantPublicId);

            return ServiceResult<List<TenantInvitationDto>>.Fail(
                ResultCodes.TenantCodes.GetTenantInvitationsException,
                "Failed to get tenant invitations.");
        }
    }

    public async Task<ServiceResult<bool>> ResendInvitationAsync(
        string tenantPublicId,
        string invitationPublicId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantPublicId))
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.TenantCodes.ResendInvitationParameterError,
                "Tenant public id is required.");
        }

        if (string.IsNullOrWhiteSpace(invitationPublicId))
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.TenantCodes.ResendInvitationParameterError,
                "Invitation public id is required.");
        }

        try
        {
            var invitationResult = await GetTenantInvitationByPublicIdAsync(
                invitationPublicId,
                cancellationToken);

            if (!invitationResult.Success || invitationResult.Data == null)
            {
                return ServiceResult<bool>.Fail(
                    ResultCodes.TenantCodes.ResendInvitationNotFound,
                    "Invitation not found.");
            }

            var invitation = invitationResult.Data;

            if (!string.Equals(
                    invitation.Tenant.PublicId.ToString(),
                    tenantPublicId,
                    StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResult<bool>.Fail(
                    ResultCodes.TenantCodes.ResendInvitationNotFound,
                    "Invitation not found.");
            }

            if (invitation.Status != InvitationStatus.Pending)
            {
                return ServiceResult<bool>.Fail(
                    ResultCodes.TenantCodes.ResendInvitationStatusInvalid,
                    "Only pending invitations can be resent.");
            }

            if (invitation.ExpiredAt <= DateTime.UtcNow)
            {
                return ServiceResult<bool>.Fail(
                    ResultCodes.TenantCodes.ResendInvitationExpired,
                    "Invitation has expired.");
            }

            var existingUser = await userRepo.GetUserByEmailAsync(
                invitation.Email,
                cancellationToken);

            if (existingUser != null)
            {
                var existingMembership = await membershipRepo.GetMembershipAsync(
                    invitation.TenantId,
                    existingUser.Id,
                    cancellationToken);

                if (existingMembership != null)
                {
                    return ServiceResult<bool>.Fail(
                        ResultCodes.TenantCodes.ResendInvitationMembershipExists,
                        "User already belongs to this tenant.");
                }
            }

            if (invitation.Status != InvitationStatus.Pending)
            {
                throw new InvalidOperationException("Only pending invitations can be resent.");
            }

            invitation.Version += 1;
            invitation.ExpiredAt = DateTime.UtcNow.AddDays(7);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await mediator.Publish(new TenantInvitationCreatedEvent(
                invitation.PublicId.ToString(),
                invitation.Tenant.Name,
                invitation.Email
            ), cancellationToken);
            await auditLogPublisher.PublishAsync(
                AuditLogTopics.MembershipInvitationResent,
                new AuditLogMessage
                {
                    TenantPublicId = tenantPublicId,
                    ActorUserPublicId = invitation.CreatedByUser.PublicId.ToString(),
                    ActorDisplayName = invitation.CreatedByUser.UserName,
                    ActionType = "Membership.InvitationResent",
                    Category = "Membership",
                    TargetType = "Invitation",
                    TargetPublicId = invitation.PublicId.ToString(),
                    TargetDisplay = invitation.Email,
                    Source = "IdentityService",
                    OccurredAtUtc = DateTime.UtcNow,
                    Metadata =
                    [
                        new AuditMetadataItem("role", invitation.Role.ToString()),
                        new AuditMetadataItem("email", invitation.Email)
                    ]
                },
                cancellationToken);
            return ServiceResult<bool>.Ok(
                true,
                ResultCodes.TenantCodes.ResendInvitationSuccess,
                "Invitation email resent successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to resend invitation {InvitationPublicId} for tenant {TenantPublicId}",
                invitationPublicId,
                tenantPublicId);

            return ServiceResult<bool>.Fail(
                ResultCodes.TenantCodes.ResendInvitationException,
                "Failed to resend invitation.");
        }
    }
}