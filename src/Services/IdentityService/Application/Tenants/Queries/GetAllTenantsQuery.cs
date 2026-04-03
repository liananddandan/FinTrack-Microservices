using IdentityService.Application.Tenants.Dtos;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Queries;

public record GetAllTenantsQuery()
    : IRequest<ServiceResult<IReadOnlyList<TenantSummaryDto>>>;