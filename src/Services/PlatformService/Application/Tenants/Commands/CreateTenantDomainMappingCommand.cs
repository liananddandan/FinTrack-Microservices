using PlatformService.Application.Tenants.Dtos;

namespace PlatformService.Application.Tenants.Commands;

using MediatR;
using SharedKernel.Common.Results;

public record CreateTenantDomainMappingCommand(
    Guid TenantPublicId,
    string Host,
    string DomainType,
    bool IsPrimary,
    bool IsActive)
    : IRequest<ServiceResult<TenantDomainMappingDto>>;