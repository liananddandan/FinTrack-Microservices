using DotNetCore.CAP;
using IdentityService.Common.DTOs;
using IdentityService.Common.Status;
using IdentityService.Events;
using IdentityService.Services.Interfaces;
using MediatR;
using SharedKernel.Common.Exceptions;
using SharedKernel.Events;
using SharedKernel.Topics;

namespace IdentityService.EventHandlers;

public class UserFirstLoginEventHandler(IJwtTokenService jwtTokenService,
    ICapPublisher capPublisher): INotificationHandler<UserFirstLoginEvent>
{
    public async Task Handle(UserFirstLoginEvent userFirstLoginEvent, CancellationToken cancellationToken)
    {
        var jwtClaimSource = new JwtClaimSource()
        {
            TenantPublicId = userFirstLoginEvent.TenantPublicId,
            JwtVersion = userFirstLoginEvent.JwtVersion,
            UserPublicId = userFirstLoginEvent.UserPublicId,
            UserRoleInTenant = userFirstLoginEvent.UserRoleInTenant
        };
        var tokenResult = await jwtTokenService.GenerateJwtTokenAsync(jwtClaimSource, JwtTokenType.FirstLoginToken);
        if (tokenResult.Data == null)
        {
            throw new TokenGenerateException("Generate token failed");
        }
        var encodedToken = Uri.EscapeDataString(tokenResult.Data);
        await capPublisher.PublishAsync(CapTopics.EmailSend, new EmailSendRequestedEvent()
        {
            To = userFirstLoginEvent.UserEmail,
            Subject = "Please change your password",
            Body = GetEmailVerificationBody(encodedToken),
            IsHtml = true
        }, new Dictionary<string, string?>(), cancellationToken);
    }
    
    private static string GetEmailVerificationBody(string changePasswordUrl)
    {
        return $"""
                 Welcome！Please click the link below to reset your password：<br/>
                 Token is {changePasswordUrl}, We will change it to a front end link later. <br/>
                 If you do not request this，please ignore this email.
                 """;
    }
}