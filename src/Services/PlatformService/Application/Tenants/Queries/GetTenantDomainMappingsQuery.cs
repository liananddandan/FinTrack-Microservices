using MediatR;
using PlatformService.Application.Tenants.Dtos;
using SharedKernel.Common.Results;

namespace PlatformService.Application.Tenants.Queries;

public record GetTenantDomainMappingsQuery(Guid TenantPublicId)
    : IRequest<ServiceResult<IReadOnlyList<TenantDomainMappingDto>>>;