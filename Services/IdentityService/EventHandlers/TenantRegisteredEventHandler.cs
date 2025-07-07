using System.Web;
using DotNetCore.CAP;
using IdentityService.Common.Status;
using IdentityService.Events;
using IdentityService.Services.Interfaces;
using MediatR;
using SharedKernel.Common.Exceptions;
using SharedKernel.Events;
using SharedKernel.Topics;

namespace IdentityService.EventHandlers;

public class TenantRegisteredEventHandler(IUserAppService userService,
    IUserVerificationService userVerificationService,
    ICapPublisher capPublisher)
    : INotificationHandler<TenantRegisteredEvent>
{
    public async Task Handle(TenantRegisteredEvent tenantRegisteredEvent, CancellationToken cancellationToken)
    {
        var getUserResult = await userService.GetUserByIdAsync(tenantRegisteredEvent.AdminUserId.ToString());
        if (getUserResult.Data == null)
        {
            throw new UserNotFoundException("Admin user not found");
        }
        
        var tokenResult = await userVerificationService.GenerateTokenAsync(getUserResult.Data, TokenPurpose.EmailConfirmation, cancellationToken);
        if (tokenResult.Data == null)
        {
            throw new TokenGenerateException("Generate token failed");
        }
        var encodedToken = Uri.EscapeDataString(tokenResult.Data);
        var encodedUserId = Uri.EscapeDataString(getUserResult.Data.PublicId.ToString());
        var confirmUrl = $"http://localhost:5100/api/account/confirm-email?token={encodedToken}&userId={encodedUserId}";
        
        await capPublisher.PublishAsync(CapTopics.EmailSend, new EmailSendRequestedEvent
        {
            To = getUserResult.Data.Email!,
            Subject = $"Please confirm your email",
            Body = GetEmailVerificationBody(confirmUrl),
            IsHtml = true
        }, new Dictionary<string, string?>(), cancellationToken);
        
    }
    
    private static string GetEmailVerificationBody(string confirmUrl)
    {
        return $"""
                Welcome！Please click the link below to verify your Email address：<br/>
                <a href="{confirmUrl}">Verify Email</a><br/>
                If you do not request this，please ignore this email.
                """;
    }
}