using IdentityService.Application.Common.Status;
using IdentityService.Application.Events;
using IdentityService.Application.Services.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using IdentityService.Infrastructure.Persistence.Repositories.Interfaces;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Services;

public class TenantInvitationService(
    ITenantRepo tenantRepo,
    IApplicationUserRepo userRepo,
    ITenantMembershipRepo membershipRepo,
    ITenantInvitationRepo invitationRepo,
    IUnitOfWork unitOfWork,
    ILogger<TenantInvitationService> logger,
    IMediator mediator) : ITenantInvitationService
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
}