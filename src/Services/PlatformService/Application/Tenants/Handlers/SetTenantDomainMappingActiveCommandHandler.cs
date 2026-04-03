using MediatR;
using PlatformService.Application.Tenants.Abstractions;
using PlatformService.Application.Tenants.Commands;
using PlatformService.Application.Tenants.Dtos;
using SharedKernel.Common.Results;

namespace PlatformService.Application.Tenants.Handlers;

public class SetTenantDomainMappingActiveCommandHandler(
    ITenantDomainMappingService service)
    : IRequestHandler<SetTenantDomainMappingActiveCommand, ServiceResult<TenantDomainMappingDto>>
{
    public Task<ServiceResult<TenantDomainMappingDto>> Handle(
        SetTenantDomainMappingActiveCommand request,
        CancellationToken cancellationToken)
    {
        return service.SetActiveAsync(request, cancellationToken);
    }
}