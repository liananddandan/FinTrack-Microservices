using MediatR;
using PlatformService.Application.Tenants.Abstractions;
using PlatformService.Application.Tenants.Commands;
using PlatformService.Application.Tenants.Dtos;
using SharedKernel.Common.Results;

namespace PlatformService.Application.Tenants.Handlers;

public class UpdateTenantDomainMappingCommandHandler(
    ITenantDomainMappingService service)
    : IRequestHandler<UpdateTenantDomainMappingCommand, ServiceResult<TenantDomainMappingDto>>
{
    public Task<ServiceResult<TenantDomainMappingDto>> Handle(
        UpdateTenantDomainMappingCommand request,
        CancellationToken cancellationToken)
    {
        return service.UpdateAsync(request, cancellationToken);
    }
}