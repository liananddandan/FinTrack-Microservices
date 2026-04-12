using IdentityService.Application.Tenants.Dtos;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Commands;

public sealed record CreateTenantStripeOnboardingLinkCommand(Guid TenantPublicId)
    : IRequest<ServiceResult<CreateTenantStripeOnboardingLinkDto>>;