using MediatR;
using PlatformService.Application.Tenants.Dtos;
using SharedKernel.Common.Results;

namespace PlatformService.Application.Tenants.Commands;

public record UpdateTenantDomainMappingCommand(
    Guid DomainPublicId,
    string Host,
    string DomainType,
    bool IsPrimary,
    bool IsActive)
    : IRequest<ServiceResult<TenantDomainMappingDto>>;