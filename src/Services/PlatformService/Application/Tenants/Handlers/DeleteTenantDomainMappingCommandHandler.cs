using MediatR;
using PlatformService.Application.Tenants.Abstractions;
using PlatformService.Application.Tenants.Commands;
using SharedKernel.Common.Results;

namespace PlatformService.Application.Tenants.Handlers;

public class DeleteTenantDomainMappingCommandHandler(
    ITenantDomainMappingService service)
    : IRequestHandler<DeleteTenantDomainMappingCommand, ServiceResult<bool>>
{
    public Task<ServiceResult<bool>> Handle(
        DeleteTenantDomainMappingCommand request,
        CancellationToken cancellationToken)
    {
        return service.DeleteAsync(request, cancellationToken);
    }
}