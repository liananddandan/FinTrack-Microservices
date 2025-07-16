using DotNetCore.CAP;
using IdentityService.Common.DTOs;
using IdentityService.Common.Status;
using IdentityService.Domain.Entities;
using IdentityService.Events;
using IdentityService.Services.Interfaces;
using MediatR;
using SharedKernel.Events;
using SharedKernel.Topics;

namespace IdentityService.EventHandlers;

public class TenantInvitationEventHandler(
    ITenantInvitationService tenantInvitationService,
    IJwtTokenService jwtTokenService,
    ICapPublisher capPublisher) 
    : INotificationHandler<TenantInvitationEvent>
{
    public async Task Handle(TenantInvitationEvent notification, CancellationToken cancellationToken)
    {
        var admin = notification.Admin;
        var emails = notification.Emails;
        foreach (var email in emails)
        {
            var emailFindResult =
                await tenantInvitationService.GetTenantInvitationByEmailAsync(email, cancellationToken);
            TenantInvitation invitation;
            if (emailFindResult.Success)
            {
                invitation = emailFindResult.Data!;
                invitation.Version += 1;
                invitation.Status = InvitationStatus.Pending;
                invitation.ExpiredAt = DateTime.UtcNow.AddDays(7);
                invitation.CreatedBy = admin.Email!;
                await tenantInvitationService.UpdateTenantInvitationAsync(invitation, cancellationToken);
            }
            else
            {
                invitation = new TenantInvitation()
                {
                    Email = email,
                    TenantPublicId = admin.Tenant!.PublicId.ToString(),
                    Role = $"User_{admin.Tenant!.Name}",
                    Status = InvitationStatus.Pending,
                    ExpiredAt = DateTime.UtcNow.AddDays(7),
                    CreatedBy = admin.Email!
                };
                await tenantInvitationService.AddTenantInvitationAsync(invitation, cancellationToken);
            }
            
            var invitationClaim = new InvitationClaimSource()
            {
                InvitationPublicId = invitation.PublicId.ToString(),
                InvitationVersion = invitation.Version.ToString(),
            };
            var token = await jwtTokenService.GenerateInvitationTokenAsync(invitationClaim);
            await capPublisher.PublishAsync(CapTopics.EmailSend, new EmailSendRequestedEvent()
            {
                To = email,
                Subject = $"Invite you add to {admin.Tenant!.Name}",
                Body = GetEmailVerificationBody(token.Data!),
                IsHtml = true
            }, new Dictionary<string, string?>(), cancellationToken);
        }
    }
    
    private static string GetEmailVerificationBody(string token)
    {
        return $"""
                Welcome！Please click the link below to receive the invitation：<br/>
                Token is {token}, and will be change to link later.<br/>
                If you do not request this，please ignore this email.
                """;
    }
}