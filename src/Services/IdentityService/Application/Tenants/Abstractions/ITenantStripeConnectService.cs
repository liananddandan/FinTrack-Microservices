using IdentityService.Application.Tenants.Dtos;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Abstractions;

public interface ITenantStripeConnectService
{
    Task<ServiceResult<TenantStripeConnectStatusDto>> GetStatusAsync(
        string tenantPublicId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CreateTenantStripeOnboardingLinkDto>> CreateOrResumeOnboardingLinkAsync(
        string tenantPublicId,
        CancellationToken cancellationToken = default);
}