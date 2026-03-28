using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Common.Status;
using IdentityService.Application.Events;
using IdentityService.Application.Services.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using IdentityService.Infrastructure.Persistence.Repositories.Interfaces;
using MediatR;
using SharedKernel.Common.Results;
using SharedKernel.Contracts.AuditLogs;
using SharedKernel.Topics;

namespace IdentityService.Application.Services;

public class TenantInvitationService(
    ITenantRepo tenantRepo,
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
                ResultCodes.Tenant.InvitationInvalidPublicId,
                "Invalid invitation public id.");
        }

        var invitation = await invitationRepo.GetByPublicIdAsync(
            parsedPublicId,
            cancellationToken);

        if (invitation == null)
        {
            return ServiceResult<TenantInvitation>.Fail(
                ResultCodes.Tenant.InvitationNotFound,
                "Invitation not found.");
        }

        return ServiceResult<TenantInvitation>.Ok(
            invitation,
            ResultCodes.Tenant.InvitationSuccess,
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
                ResultCodes.Tenant.CreateInvitationParameterError,
                "Tenant public id is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Tenant.CreateInvitationParameterError,
                "Email is required.");
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Tenant.CreateInvitationParameterError,
                "Role is required.");
        }

        email = email.Trim().ToLowerInvariant();

        try
        {
            var tenant = await tenantRepo.GetTenantByPublicIdAsync(
                tenantPublicId,
                cancellationToken);

            if (tenant == null)
            {
                return ServiceResult<bool>.Fail(
                    ResultCodes.Tenant.CreateInvitationTenantNotFound,
                    "Tenant not found.");
            }

            var user = await userRepo.GetUserByEmailAsync(email, cancellationToken);
            if (user == null)
            {
                return ServiceResult<bool>.Fail(
                    ResultCodes.Tenant.CreateInvitationUserNotFound,
                    "User not found.");
            }

            var existingMembership = await membershipRepo.GetMembershipAsync(
                tenant.Id,
                user.Id,
                cancellationToken);

            if (existingMembership != null)
            {
                return ServiceResult<bool>.Fail(
                    ResultCodes.Tenant.CreateInvitationUserAlreadyExists,
                    "User already belongs to this tenant.");
            }

            if (!Guid.TryParse(invitedByUserPublicId, out var inviterPublicId))
            {
                return ServiceResult<bool>.Fail(
                    ResultCodes.Tenant.CreateInvitationParameterError,
                    "Invalid inviter.");
            }

            var inviter = await userRepo.GetUserByPublicIdAsync(
                invitedByUserPublicId,
                cancellationToken);

            if (inviter == null)
            {
                return ServiceResult<bool>.Fail(
                    ResultCodes.Tenant.CreateInvitationInviterNotFound,
                    "Inviter not found.");
            }

            if (!Enum.TryParse<TenantRole>(role, true, out var parsedRole))
            {
                return ServiceResult<bool>.Fail(
                    ResultCodes.Tenant.CreateInvitationParameterError,
                    "Invalid role.");
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

            await invitationRepo.AddAsync(invitation, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await mediator.Publish(new TenantInvitationCreatedEvent(
                invitation.PublicId.ToString(),
                tenant.Name,
                email
            ), cancellationToken);

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

            return ServiceResult<bool>.Ok(
                true,
                ResultCodes.Tenant.CreateInvitationSuccess,
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
                ResultCodes.Tenant.CreateInvitationException,
                "Failed to create invitation.");
        }
    }

    public async Task<ServiceResult<ResolveTenantInvitationResult>> ResolveInvitationAsync(
        string invitationPublicId,
        string invitationVersion,
        CancellationToken cancellationToken = default)
    {
        var invitationResult = await GetTenantInvitationByPublicIdAsync(
            invitationPublicId,
            cancellationToken);

        if (!invitationResult.Success || invitationResult.Data == null)
        {
            return ServiceResult<ResolveTenantInvitationResult>.Fail(
                ResultCodes.Tenant.ResolveInvitationNotFound,
                "Invitation not found.");
        }

        var invitation = invitationResult.Data;

        if (!int.TryParse(invitationVersion, out var parsedVersion) ||
            parsedVersion != invitation.Version)
        {
            return ServiceResult<ResolveTenantInvitationResult>.Fail(
                ResultCodes.Tenant.ResolveInvitationVersionInvalid,
                "Invitation version is invalid.");
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            return ServiceResult<ResolveTenantInvitationResult>.Fail(
                ResultCodes.Tenant.ResolveInvitationStatusInvalid,
                "Invitation is no longer available.");
        }

        if (invitation.ExpiredAt <= DateTime.UtcNow)
        {
            return ServiceResult<ResolveTenantInvitationResult>.Fail(
                ResultCodes.Tenant.ResolveInvitationExpired,
                "Invitation has expired.");
        }

        var result = new ResolveTenantInvitationResult(
            invitation.PublicId.ToString(),
            invitation.Tenant.Name,
            invitation.Email,
            invitation.Role.ToString(),
            invitation.Status.ToString(),
            invitation.ExpiredAt);

        return ServiceResult<ResolveTenantInvitationResult>.Ok(
            result,
            ResultCodes.Tenant.ResolveInvitationSuccess,
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
                ResultCodes.Tenant.AcceptInvitationNotFound,
                "Invitation not found.");
        }

        var invitation = invitationResult.Data;

        if (!int.TryParse(invitationVersion, out var parsedVersion) ||
            parsedVersion != invitation.Version)
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Tenant.AcceptInvitationVersionInvalid,
                "Invitation version is invalid.");
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Tenant.AcceptInvitationStatusInvalid,
                "Invitation is no longer available.");
        }

        if (invitation.ExpiredAt <= DateTime.UtcNow)
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Tenant.AcceptInvitationExpired,
                "Invitation has expired.");
        }

        var user = await userRepo.GetUserByEmailAsync(invitation.Email, cancellationToken);
        if (user == null)
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Tenant.AcceptInvitationUserNotFound,
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
                    ResultCodes.Tenant.AcceptInvitationMembershipExists,
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
            ResultCodes.Tenant.AcceptInvitationSuccess,
            "Invitation accepted successfully.");
    }

    public async Task<ServiceResult<List<TenantInvitationDto>>> GetTenantInvitationsAsync(
        string tenantPublicId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantPublicId))
        {
            return ServiceResult<List<TenantInvitationDto>>.Fail(
                ResultCodes.Tenant.GetTenantInvitationsParameterError,
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
                ResultCodes.Tenant.GetTenantInvitationsSuccess,
                "Tenant invitations fetched successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to get tenant invitations for tenant {TenantPublicId}",
                tenantPublicId);

            return ServiceResult<List<TenantInvitationDto>>.Fail(
                ResultCodes.Tenant.GetTenantInvitationsException,
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
                ResultCodes.Tenant.ResendInvitationParameterError,
                "Tenant public id is required.");
        }

        if (string.IsNullOrWhiteSpace(invitationPublicId))
        {
            return ServiceResult<bool>.Fail(
                ResultCodes.Tenant.ResendInvitationParameterError,
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
                    ResultCodes.Tenant.ResendInvitationNotFound,
                    "Invitation not found.");
            }

            var invitation = invitationResult.Data;

            if (!string.Equals(
                    invitation.Tenant.PublicId.ToString(),
                    tenantPublicId,
                    StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResult<bool>.Fail(
                    ResultCodes.Tenant.ResendInvitationNotFound,
                    "Invitation not found.");
            }

            if (invitation.Status != InvitationStatus.Pending)
            {
                return ServiceResult<bool>.Fail(
                    ResultCodes.Tenant.ResendInvitationStatusInvalid,
                    "Only pending invitations can be resent.");
            }

            if (invitation.ExpiredAt <= DateTime.UtcNow)
            {
                return ServiceResult<bool>.Fail(
                    ResultCodes.Tenant.ResendInvitationExpired,
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
                        ResultCodes.Tenant.ResendInvitationMembershipExists,
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
                ResultCodes.Tenant.ResendInvitationSuccess,
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
                ResultCodes.Tenant.ResendInvitationException,
                "Failed to resend invitation.");
        }
    }
}