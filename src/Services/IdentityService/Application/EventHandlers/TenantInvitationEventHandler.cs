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
    public Task Handle(TenantInvitationEvent notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}