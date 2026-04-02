using PlatformService.Application.Tenants.Abstractions;
using PlatformService.Application.Tenants.Dtos;
using PlatformService.Application.Tenants.Queries;
using MediatR;
using SharedKernel.Common.Results;

namespace PlatformService.Application.Tenants.Handlers;

public class GetTenantDomainMappingsQueryHandler(
    ITenantDomainMappingService service)
    : IRequestHandler<GetTenantDomainMappingsQuery, ServiceResult<IReadOnlyList<TenantDomainMappingDto>>>
{
    public Task<ServiceResult<IReadOnlyList<TenantDomainMappingDto>>> Handle(
        GetTenantDomainMappingsQuery request,
        CancellationToken cancellationToken)
    {
        return service.GetByTenantAsync(request.TenantPublicId, cancellationToken);
    }
}