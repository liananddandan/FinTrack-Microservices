using MediatR;
using PlatformService.Application.Tenants.Abstractions;
using PlatformService.Application.Tenants.Commands;
using PlatformService.Application.Tenants.Dtos;
using SharedKernel.Common.Results;

namespace PlatformService.Application.Tenants.Handlers;


public class CreateTenantDomainMappingCommandHandler(
    ITenantDomainMappingService service)
    : IRequestHandler<CreateTenantDomainMappingCommand, ServiceResult<TenantDomainMappingDto>>
{
    public Task<ServiceResult<TenantDomainMappingDto>> Handle(
        CreateTenantDomainMappingCommand request,
        CancellationToken cancellationToken)
    {
        return service.CreateAsync(request, cancellationToken);
    }
}