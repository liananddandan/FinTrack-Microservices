using IdentityService.Application.Tenants.Dtos;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Queries;

public sealed record GetTenantStripeConnectStatusQuery(Guid TenantPublicId)
    : IRequest<ServiceResult<TenantStripeConnectStatusDto>>;