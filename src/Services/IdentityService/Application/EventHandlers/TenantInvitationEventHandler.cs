using DotNetCore.CAP;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Common.Status;
using IdentityService.Application.Events;
using IdentityService.Application.Services.Interfaces;
using IdentityService.Domain.Entities;
using MediatR;
using SharedKernel.Events;
using SharedKernel.Topics;

namespace IdentityService.Application.EventHandlers;

public class TenantInvitationEventHandler(
    ITenantInvitationService tenantInvitationService,
    IJwtTokenService jwtTokenService,
    IUserDomainService userDomainService,
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
                var roleName = $"User_{admin.Tenant!.Name}";
                var roleCreateState = await userDomainService.CreateRoleInnerAsync(roleName, cancellationToken);
                if (roleCreateState != RoleStatus.CreateSuccess && roleCreateState != RoleStatus.RoleAlreadyExist)
                {
                    continue;
                }

                invitation = new TenantInvitation()
                {
                    Email = email,
                    TenantPublicId = admin.Tenant!.PublicId.ToString(),
                    Role = roleName,
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