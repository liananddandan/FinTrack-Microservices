using MediatR;
using PlatformService.Application.Tenants.Dtos;
using SharedKernel.Common.Results;

namespace PlatformService.Application.Tenants.Commands;

public record SetTenantDomainMappingActiveCommand(
    Guid DomainPublicId,
    bool IsActive)
    : IRequest<ServiceResult<TenantDomainMappingDto>>;