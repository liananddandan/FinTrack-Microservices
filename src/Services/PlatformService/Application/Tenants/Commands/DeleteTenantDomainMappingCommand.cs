using MediatR;
using SharedKernel.Common.Results;

namespace PlatformService.Application.Tenants.Commands;


public record DeleteTenantDomainMappingCommand(Guid DomainPublicId)
    : IRequest<ServiceResult<bool>>;