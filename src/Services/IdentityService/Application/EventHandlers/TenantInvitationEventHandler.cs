using DotNetCore.CAP;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Events;
using IdentityService.Application.Services.Interfaces;
using MediatR;
using SharedKernel.Events;
using SharedKernel.Topics;

namespace IdentityService.Application.EventHandlers;

public class TenantInvitationEventHandler(
    ITenantInvitationService tenantInvitationService,
    IJwtTokenService jwtTokenService,
    ICapPublisher capPublisher,
    ILogger<TenantInvitationEventHandler> logger)
    : INotificationHandler<TenantInvitationCreatedEvent>
{
    public async Task Handle(TenantInvitationCreatedEvent notification, CancellationToken cancellationToken)
    {
        var invitationResult = await tenantInvitationService
            .GetTenantInvitationByPublicIdAsync(notification.InvitationPublicId, cancellationToken);

        if (!invitationResult.Success || invitationResult.Data == null)
        {
            logger.LogWarning(
                "Could not publish tenant invitation email event because invitation {InvitationPublicId} was not found.",
                notification.InvitationPublicId);
            return;
        }

        var invitation = invitationResult.Data;

        var tokenResult = await jwtTokenService.GenerateInvitationTokenAsync(
            new InvitationClaimSource
            {
                InvitationPublicId = invitation.PublicId.ToString(),
                InvitationVersion = invitation.Version.ToString()
            });

        if (!tokenResult.Success || string.IsNullOrWhiteSpace(tokenResult.Data))
        {
            logger.LogWarning(
                "Could not generate invitation token for invitation {InvitationPublicId}.",
                notification.InvitationPublicId);
            return;
        }

        var invitationToken = tokenResult.Data;

        var invitationLink =
            $"http://localhost:5174/invitations/accept?token={Uri.EscapeDataString(invitationToken)}";

        var message = new TenantInvitationEmailRequestedEvent(
            invitation.Email,
            invitation.Tenant.Name,
            invitationLink,
            invitation.Role.ToString()
        );

        await capPublisher.PublishAsync(
            NotificationTopics.TenantInvitationEmailRequested,
            message,
            cancellationToken: cancellationToken);

        logger.LogInformation(
            "Published tenant invitation email event for invitation {InvitationPublicId} to {Email}.",
            notification.InvitationPublicId,
            invitation.Email);
    }
}