using IdentityService.Application.Common.Abstractions;
using IdentityService.Application.Tenants.Abstractions;
using IdentityService.Application.Tenants.Dtos;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Tenants.Services;

public class TenantStripeConnectService(
    ITenantRepository tenantRepository,
    IStripeConnectService stripeConnectService,
    IUnitOfWork unitOfWork,
    ILogger<TenantStripeConnectService> logger)
    : ITenantStripeConnectService
{
    public async Task<ServiceResult<TenantStripeConnectStatusDto>> GetStatusAsync(
        string tenantPublicId,
        CancellationToken cancellationToken = default)
    {
        var tenant = await tenantRepository.GetTenantByPublicIdAsync(
            tenantPublicId,
            cancellationToken);

        if (tenant is null)
        {
            return ServiceResult<TenantStripeConnectStatusDto>.Fail(
                "TENANT_NOT_FOUND",
                "Tenant not found.");
        }

        if (string.IsNullOrWhiteSpace(tenant.StripeConnectedAccountId))
        {
            return ServiceResult<TenantStripeConnectStatusDto>.Ok(
                new TenantStripeConnectStatusDto(
                    null,
                    false,
                    false,
                    false,
                    true),
                "TENANT_STRIPE_CONNECT_STATUS_OK",
                "Tenant Stripe connect status loaded.");
        }

        var statusResult = await stripeConnectService.GetAccountStatusAsync(
            tenant.StripeConnectedAccountId,
            cancellationToken);

        if (!statusResult.Success || statusResult.Data is null)
        {
            logger.LogWarning(
                "Failed to get Stripe connected account status for tenant {TenantPublicId}. Code: {Code}, Message: {Message}",
                tenantPublicId,
                statusResult.Code,
                statusResult.Message);

            return ServiceResult<TenantStripeConnectStatusDto>.Ok(
                new TenantStripeConnectStatusDto(
                    tenant.StripeConnectedAccountId,
                    tenant.StripeChargeEnabled,
                    tenant.StripePayoutsEnabled,
                    true,
                    !tenant.StripeChargeEnabled),
                "TENANT_STRIPE_CONNECT_STATUS_OK",
                "Tenant Stripe connect status loaded.");
        }

        tenant.StripeChargeEnabled = statusResult.Data.ChargesEnabled;
        tenant.StripePayoutsEnabled = statusResult.Data.PayoutsEnabled;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<TenantStripeConnectStatusDto>.Ok(
            new TenantStripeConnectStatusDto(
                tenant.StripeConnectedAccountId,
                tenant.StripeChargeEnabled,
                tenant.StripePayoutsEnabled,
                true,
                !tenant.StripeChargeEnabled),
            "TENANT_STRIPE_CONNECT_STATUS_OK",
            "Tenant Stripe connect status loaded.");
    }

    public async Task<ServiceResult<CreateTenantStripeOnboardingLinkDto>> CreateOrResumeOnboardingLinkAsync(
        string tenantPublicId,
        CancellationToken cancellationToken = default)
    {
        var tenant = await tenantRepository.GetTenantByPublicIdAsync(
            tenantPublicId,
            cancellationToken);

        if (tenant is null)
        {
            return ServiceResult<CreateTenantStripeOnboardingLinkDto>.Fail(
                "TENANT_NOT_FOUND",
                "Tenant not found.");
        }

        if (string.IsNullOrWhiteSpace(tenant.StripeConnectedAccountId))
        {
            var createAccountResult = await stripeConnectService.CreateConnectedAccountAsync(
                cancellationToken);

            if (!createAccountResult.Success || createAccountResult.Data is null)
            {
                return ServiceResult<CreateTenantStripeOnboardingLinkDto>.Fail(
                    createAccountResult.Code ?? "STRIPE_CONNECTED_ACCOUNT_CREATE_FAILED",
                    createAccountResult.Message ?? "Failed to create Stripe connected account.");
            }

            tenant.StripeConnectedAccountId = createAccountResult.Data.ConnectedAccountId;
            tenant.StripeChargeEnabled = createAccountResult.Data.ChargesEnabled;
            tenant.StripePayoutsEnabled = createAccountResult.Data.PayoutsEnabled;

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var onboardingLinkResult = await stripeConnectService.CreateOnboardingLinkAsync(
            tenant.StripeConnectedAccountId!,
            cancellationToken);

        if (!onboardingLinkResult.Success || onboardingLinkResult.Data is null)
        {
            return ServiceResult<CreateTenantStripeOnboardingLinkDto>.Fail(
                onboardingLinkResult.Code ?? "STRIPE_ONBOARDING_LINK_CREATE_FAILED",
                onboardingLinkResult.Message ?? "Failed to create Stripe onboarding link.");
        }

        return ServiceResult<CreateTenantStripeOnboardingLinkDto>.Ok(
            new CreateTenantStripeOnboardingLinkDto(onboardingLinkResult.Data.Url),
            "TENANT_STRIPE_ONBOARDING_LINK_CREATED",
            "Tenant Stripe onboarding link created successfully.");
    }
}