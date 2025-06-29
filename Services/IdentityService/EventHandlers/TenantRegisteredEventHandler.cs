using IdentityService.Events;
using MediatR;

namespace IdentityService.EventHandlers;

public class TenantRegisteredEventHandler : INotificationHandler<TenantRegisteredEvent>
{
    public Task Handle(TenantRegisteredEvent notification, CancellationToken cancellationToken)
    {
        // todo: finish tenant register event
        throw new NotImplementedException();
    }
}