using IdentityService.Application.Tenants.Abstractions;
using IdentityService.Application.Tenants.Commands;
using IdentityService.Application.Tenants.Dtos;
using MediatR;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Handlers;

public class CreateTenantStripeOnboardingLinkCommandHandler(
    ITenantStripeConnectService tenantStripeConnectService)
    : IRequestHandler<CreateTenantStripeOnboardingLinkCommand, ServiceResult<CreateTenantStripeOnboardingLinkDto>>
{
    public async Task<ServiceResult<CreateTenantStripeOnboardingLinkDto>> Handle(
        CreateTenantStripeOnboardingLinkCommand request,
        CancellationToken cancellationToken)
    {
        return await tenantStripeConnectService.CreateOrResumeOnboardingLinkAsync(
            request.TenantPublicId.ToString(),
            cancellationToken);
    }
}