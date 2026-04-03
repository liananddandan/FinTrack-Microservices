using MediatR;
using PlatformService.Application.Tenants.Dtos;
using SharedKernel.Common.Results;

namespace PlatformService.Application.Tenants.Queries;

public record GetAllPlatformTenantsQuery()
    : IRequest<ServiceResult<IReadOnlyList<TenantSummaryDto>>>;