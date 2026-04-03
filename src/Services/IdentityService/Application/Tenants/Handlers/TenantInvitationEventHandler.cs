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
        logger.LogInformation(
            "Handling TenantInvitationCreatedEvent. InvitationPublicId={InvitationPublicId}",
            notification.InvitationPublicId);

        var invitationResult = await tenantInvitationService
            .GetTenantInvitationByPublicIdAsync(notification.InvitationPublicId, cancellationToken);

        logger.LogInformation(
            "Tenant invitation lookup completed. InvitationPublicId={InvitationPublicId}, Success={Success}, HasData={HasData}",
            notification.InvitationPublicId,
            invitationResult.Success,
            invitationResult.Data is not null);

        if (!invitationResult.Success || invitationResult.Data == null)
        {
            logger.LogWarning(
                "Could not publish tenant invitation email event because invitation {InvitationPublicId} was not found.",
                notification.InvitationPublicId);
            return;
        }

        var invitation = invitationResult.Data;

        logger.LogInformation(
            "Preparing invitation email event. InvitationPublicId={InvitationPublicId}, Email={Email}, TenantName={TenantName}, Role={Role}",
            notification.InvitationPublicId,
            invitation.Email,
            invitation.Tenant.Name,
            invitation.Role);

        string invitationToken;

        try
        {
            logger.LogInformation(
                "Generating invitation token for InvitationPublicId={InvitationPublicId}",
                notification.InvitationPublicId);

            invitationToken = jwtTokenService.GenerateInvitationToken(invitation);

            logger.LogInformation(
                "Invitation token generated successfully for InvitationPublicId={InvitationPublicId}. TokenLength={TokenLength}",
                notification.InvitationPublicId,
                invitationToken?.Length ?? 0);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Could not generate invitation token for invitation {InvitationPublicId}.",
                notification.InvitationPublicId);
            return;
        }

        if (string.IsNullOrWhiteSpace(invitationToken))
        {
            logger.LogWarning(
                "Generated invitation token is empty for invitation {InvitationPublicId}.",
                notification.InvitationPublicId);
            return;
        }

        var portalBaseUrl = configuration["Frontend:PortalBaseUrl"];

        logger.LogInformation(
            "Resolved Frontend:PortalBaseUrl for InvitationPublicId={InvitationPublicId}. Value={PortalBaseUrl}",
            notification.InvitationPublicId,
            portalBaseUrl);

        if (string.IsNullOrWhiteSpace(portalBaseUrl))
        {
            logger.LogError(
                "Frontend:PortalBaseUrl is not configured. Invitation link cannot be generated for invitation {InvitationPublicId}.",
                notification.InvitationPublicId);
            return;
        }

        var invitationLink =
            $"{portalBaseUrl.TrimEnd('/')}/account/accept-invitation?token={Uri.EscapeDataString(invitationToken)}";

        logger.LogInformation(
            "Generated invitation link for InvitationPublicId={InvitationPublicId}. InvitationLink={InvitationLink}",
            notification.InvitationPublicId,
            invitationLink);

        var message = new TenantInvitationEmailRequestedEvent(
            invitation.Email,
            invitation.Tenant.Name,
            invitationLink,
            invitation.Role.ToString()
        );

        try
        {
            logger.LogInformation(
                "Publishing CAP message. Topic={Topic}, InvitationPublicId={InvitationPublicId}, Email={Email}",
                NotificationTopics.TenantInvitationEmailRequested,
                notification.InvitationPublicId,
                invitation.Email);

            await capPublisher.PublishAsync(
                NotificationTopics.TenantInvitationEmailRequested,
                message,
                cancellationToken: cancellationToken);

            logger.LogInformation(
                "Published tenant invitation email event successfully. Topic={Topic}, InvitationPublicId={InvitationPublicId}, Email={Email}",
                NotificationTopics.TenantInvitationEmailRequested,
                notification.InvitationPublicId,
                invitation.Email);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to publish tenant invitation email event. Topic={Topic}, InvitationPublicId={InvitationPublicId}, Email={Email}",
                NotificationTopics.TenantInvitationEmailRequested,
                notification.InvitationPublicId,
                invitation.Email);
            throw;
        }
    }
}