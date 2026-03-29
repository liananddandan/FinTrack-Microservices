using DotNetCore.CAP;
using IdentityService.Application.Common.Abstractions;
using IdentityService.Application.Tenants.Abstractions;
using IdentityService.Application.Tenants.Events;
using MediatR;
using SharedKernel.Events;
using SharedKernel.Topics;

namespace IdentityService.Application.Tenants.Handlers;

public class TenantInvitationEventHandler(
    ITenantInvitationService tenantInvitationService,
    IJwtTokenService jwtTokenService,
    ICapPublisher capPublisher,
    ILogger<TenantInvitationEventHandler> logger,
    IConfiguration configuration)
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

        string invitationToken;
        
        try
        {
            invitationToken = jwtTokenService.GenerateInvitationToken(invitation);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Could not generate invitation token for invitation {InvitationPublicId}.",
                notification.InvitationPublicId);
            return;
        }
        
        if (string.IsNullOrWhiteSpace(invitationToken))
        {
            logger.LogWarning(
                "Could not generate invitation token for invitation {InvitationPublicId}.",
                notification.InvitationPublicId);
            return;
        }
        var portalBaseUrl = configuration["Frontend:PortalBaseUrl"];

        if (string.IsNullOrWhiteSpace(portalBaseUrl))
        {
            logger.LogError(
                "Frontend:PortalBaseUrl is not configured. Invitation link cannot be generated for invitation {InvitationPublicId}.",
                notification.InvitationPublicId);
            return;
        }

        var invitationLink =
            $"{portalBaseUrl.TrimEnd('/')}/invitations/accept?token={Uri.EscapeDataString(invitationToken)}";

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